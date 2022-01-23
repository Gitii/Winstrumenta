using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Community.Archives.Rpm;
using Community.Wsl.Sdk;

namespace PackageInstaller.Core.Services;

public class Rpm : IRpm
{
    private readonly IWslCommands _wslCommands;

    public Rpm(IWslCommands wslCommands)
    {
        _wslCommands = wslCommands;
    }

    public async Task<bool> IsPackageInstalledAsync(string distroName, string packageName)
    {
        var result = await _wslCommands
            .ExecuteCommandAsync(
                distroName,
                "rpm",
                new[] { "-q", packageName },
                true,
                ignoreExitCode: true
            )
            .ConfigureAwait(false);

        if (result == $"package {packageName} is not installed")
        {
            return false;
        }

        return true;
    }

    public async Task<IPlatformDependentPackageManager.PackageInfo> GetInstalledPackageInfoAsync(
        string distroName,
        string packageName
    )
    {
        var result = await _wslCommands
            .ExecuteCommandAsync(
                distroName,
                "rpm",
                new[] { "-q", packageName, "--queryformat", "%{version}" },
                true
            )
            .ConfigureAwait(false);

        if (result == $"package {packageName} is not installed")
        {
            throw new Exception($"Package '{packageName}' is not installed!");
        }

        return new IPlatformDependentPackageManager.PackageInfo()
        {
            Name = packageName,
            Version = result
        };
    }

    public async Task<IPlatformDependentPackageManager.PackageMetaData> ExtractPackageMetaDataAsync(
        FileSystemPath filePath
    )
    {
        var reader = new RpmArchiveReader();
        var stream = File.OpenRead(filePath.WindowsPath);
        await using var _ = stream.ConfigureAwait(false);
        var md = await reader.GetMetaDataAsync(stream).ConfigureAwait(false);

        var package = md.Package;
        var parser = new RpmPackageNameParser();
        if (parser.TryParse(package, out var parsedPackageName))
        {
            package = parsedPackageName!.Value.Name;
        }

        return new IPlatformDependentPackageManager.PackageMetaData()
        {
            Package = package,
            Architecture = md.Architecture,
            Description = md.Description,
            Version = md.Version,
            AllFields = md.AllFields,
            IconName = null,
        };
    }

    public async Task<(bool isSupported, string? reason)> IsPackageSupportedAsync(
        FileSystemPath filePath
    )
    {
        try
        {
            var reader = new RpmArchiveReader();
            var stream = File.OpenRead(filePath.WindowsPath);
            await using var __ = stream.ConfigureAwait(false);
            _ = await reader.GetMetaDataAsync(stream).ConfigureAwait(false);

            return (true, null);
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
    }

    public IPlatformDependentPackageManager.PackageInstallationStatus CompareVersions(
        string baseVersion,
        string otherVersion
    )
    {
        var cmp = RpmVersion.Compare(baseVersion, otherVersion);
        if (cmp == 0)
        {
            return IPlatformDependentPackageManager.PackageInstallationStatus.InstalledSameVersion;
        }

        if (cmp < 0)
        {
            return IPlatformDependentPackageManager.PackageInstallationStatus.InstalledOlderVersion;
        }

        return IPlatformDependentPackageManager.PackageInstallationStatus.InstalledNewerVersion;
    }

    private async Task<(bool success, string logs)> ExecuteRpmAsync(
        string distroName,
        params string[] arguments
    )
    {
        var cmd = _wslCommands.CreateCommand(
            distroName,
            "rpm",
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

        var taskOutput = Dpkg.ReadLines(pipes.StandardOutput!)
            .Merge(Dpkg.ReadLines(pipes.StandardError!))
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
        return ExecuteRpmAsync(distroName, "-v", "--install", filePath.UnixPath);
    }

    public Task<(bool success, string logs)> UninstallAsync(string distroName, string packageName)
    {
        return ExecuteRpmAsync(distroName, "-v", "--erase", packageName);
    }

    public Task<(bool success, string logs)> UpgradeAsync(
        string distroName,
        FileSystemPath filePath
    )
    {
        return ExecuteRpmAsync(distroName, "-v", "--upgrade", filePath.UnixPath);
    }

    public Task<(bool success, string logs)> DowngradeAsync(
        string distroName,
        FileSystemPath filePath
    )
    {
        return ExecuteRpmAsync(distroName, "-v", "--upgrade", "--oldpackage", filePath.UnixPath);
    }

    public Task<bool> IsSupportedByDistributionAsync(string distroName)
    {
        return _wslCommands.CheckCommandExistsAsync(distroName, "rpm");
    }
}
