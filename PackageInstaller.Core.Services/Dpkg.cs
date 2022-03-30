using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Community.Wsl.Sdk;
using ReactiveUI;

namespace PackageInstaller.Core.Services;

public class Dpkg : IDpkg
{
    private readonly IWslCommands _wslCommands;
    private IDebianPackageReader _debianPackageReader;

    public Dpkg(IWslCommands wslCommands, IDebianPackageReader debianPackageReader)
    {
        _wslCommands = wslCommands;
        _debianPackageReader = debianPackageReader;
    }

    public async Task<bool> IsPackageInstalledAsync(string distroName, string packageName)
    {
        var output = await _wslCommands
            .ExecuteCommandAsync(
                distroName,
                "dpkg",
                new[] { "-s", packageName },
                ignoreExitCode: true,
                includeStandardError: true
            )
            .ConfigureAwait(false);

        return !output.Contains(
            $"dpkg-query: package '{packageName}' is not installed",
            StringComparison.Ordinal
        );
    }

    public async Task<IPlatformDependentPackageManager.PackageMetaData> ExtractPackageMetaDataAsync(
        FileSystemPath filePath
    )
    {
        var data = await _debianPackageReader.ReadMetaDataAsync(filePath).ConfigureAwait(false);

        string label;
        string description;
        if (data.Description.Contains("\n\n", StringComparison.Ordinal))
        {
            var labelLength = data.Description.IndexOf("\n\n", StringComparison.Ordinal);
            label = data.Description.Substring(0, labelLength);
            description = data.Description.Substring(labelLength + 2);
        }
        else
        {
            label = data.Package;
            description = data.Description;
        }

        return new IPlatformDependentPackageManager.PackageMetaData()
        {
            PackageName = data.Package,
            PackageLabel = label,
            Description = description,
            VersionCode = data.Version,
            VersionLabel = data.Version,
            Architecture = data.Architecture,
            IconName = data.IconName,
            AllFields = data.AllFields
        };
    }

    public Task<(bool isSupported, string? reason)> IsPackageSupportedAsync(FileSystemPath filePath)
    {
        return _debianPackageReader.IsSupportedAsync(filePath);
    }

    public async Task<IPlatformDependentPackageManager.PackageInfo> GetInstalledPackageInfoAsync(
        string distroName,
        string packageName
    )
    {
        var output = await _wslCommands
            .ExecuteCommandAsync(
                distroName,
                "dpkg",
                new[] { "-s", packageName },
                ignoreExitCode: true
            )
            .ConfigureAwait(false);

        if (
            output.Contains(
                $"dpkg-query: package '{packageName}' is not installed",
                StringComparison.Ordinal
            )
        )
        {
            throw new Exception($"Package '{packageName}' is not installed!");
        }
        else if (!output.Contains($"Package: {packageName}\n", StringComparison.Ordinal))
        {
            throw new Exception($"dpkg returned malformed or wrong package details.");
        }
        else
        {
            ControlFile cf = new ControlFile();
            cf.Parse(output);

            return new IDpkg.PackageInfo()
            {
                Name = packageName,
                VersionCode = cf.GetEntryContent("Version")
            };
        }
    }

    public IPlatformDependentPackageManager.PackageInstallationStatus CompareVersions(
        string versionA,
        string versionB
    )
    {
        var pva = new DebianVersion(versionA);
        var pvb = new DebianVersion(versionB);

        return GetInstallationStatusFromComparison(pva, pvb);
    }

    public IPlatformDependentPackageManager.PackageInstallationStatus GetInstallationStatusFromComparison(
        DebianVersion left,
        DebianVersion right
    )
    {
        var versionComparison = left.CompareTo(right);

        if (versionComparison < 0)
        {
            return IPlatformDependentPackageManager.PackageInstallationStatus.InstalledOlderVersion;
        }
        else if (versionComparison > 0)
        {
            return IPlatformDependentPackageManager.PackageInstallationStatus.InstalledNewerVersion;
        }
        else
        {
            return IPlatformDependentPackageManager.PackageInstallationStatus.InstalledSameVersion;
        }
    }

    private async Task<(bool success, string logs)> ExecuteDpkgAsync(
        string distroName,
        params string[] arguments
    )
    {
        var cmd = _wslCommands.CreateCommand(
            distroName,
            "dpkg",
            arguments,
            new CommandExecutionOptions()
            {
                FailOnNegativeExitCode = false,
                StdErrDataProcessingMode = DataProcessingMode.External,
                StdoutDataProcessingMode = DataProcessingMode.External
            },
            true
        );

        var pipes = cmd.Start();

        var taskOutput = ReadLines(pipes.StandardOutput!)
            .Merge(ReadLines(pipes.StandardError!))
            .ToArray()
            .ToTask();

        var results = await cmd.WaitAndGetResultsAsync().ConfigureAwait(false); // wait for command to finish

        var output = await taskOutput.ConfigureAwait(false); // wait for output to be fetched

        return (results.ExitCode == 0, String.Join(Environment.NewLine, output));
    }

    public Task<(bool success, string logs)> InstallAsync(
        string distroName,
        FileSystemPath filePath
    )
    {
        return ExecuteDpkgAsync(
            distroName,
            "--force-confold",
            "--force-confdef",
            "-i",
            filePath.UnixPath
        );
    }

    public Task<(bool success, string logs)> UninstallAsync(string distroName, string packageName)
    {
        return ExecuteDpkgAsync(distroName, "-r", packageName);
    }

    public Task<(bool success, string logs)> UpgradeAsync(
        string distroName,
        FileSystemPath filePath
    )
    {
        return InstallAsync(distroName, filePath);
    }

    public Task<(bool success, string logs)> DowngradeAsync(
        string distroName,
        FileSystemPath filePath
    )
    {
        return ExecuteDpkgAsync(
            distroName,
            "--force-confold",
            "--force-confdef",
            "--force-downgrade",
            "-i",
            filePath.UnixPath
        );
    }

    public async Task<bool> IsSupportedByDistributionAsync(string distroName, string distroOrigin)
    {
        return distroOrigin == WslProvider.ORIGIN_WSL
            && await _wslCommands.CheckCommandExistsAsync(distroName, "dpkg");
    }

    public static IObservable<string> ReadLines(StreamReader reader)
    {
        return Observable.Using(
            () => reader,
            r =>
                Observable
                    .FromAsync(r.ReadLineAsync, RxApp.TaskpoolScheduler)
                    .Repeat()
                    .TakeWhile(line => line != null)
                    .Select(
                        (line) => (line ?? String.Empty).Replace("\0", "", StringComparison.Ordinal)
                    )
                    .Where((line) => line.Length != 0)
        );
    }
}
