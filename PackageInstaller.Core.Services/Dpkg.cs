using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Community.Wsl.Sdk;
using ReactiveUI;

namespace PackageInstaller.Core.Services
{
    public class Dpkg : IDpkg
    {
        private readonly IWslCommands _wslCommands;
        private IDebianPackageReader _debianPackageReader;

        public Dpkg(IWslCommands wslCommands, IDebianPackageReader debianPackageReader)
        {
            _wslCommands = wslCommands;
            _debianPackageReader = debianPackageReader;
        }

        public async Task<bool> IsPackageInstalled(string distroName, string packageName)
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

            return !output.Contains($"dpkg-query: package '{packageName}' is not installed");
        }

        public async Task<IPlatformDependentPackageManager.PackageMetaData> ExtractPackageMetaData(
            FileSystemPath filePath
        )
        {
            var data = await Task.Run(
                async () => await _debianPackageReader.ReadMetaData(filePath)
            );

            return new IPlatformDependentPackageManager.PackageMetaData()
            {
                Package = data.Package,
                Description = data.Description,
                Version = data.Version,
                Architecture = data.Architecture,
                IconName = data.IconName,
                AllFields = data.AllFields
            };
        }

        public Task<(bool isSupported, string? reason)> IsPackageSupported(FileSystemPath filePath)
        {
            return _debianPackageReader.IsSupported(filePath);
        }

        public async Task<IPlatformDependentPackageManager.PackageInfo> GetInstalledPackageInfo(
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

            if (output.Contains($"dpkg-query: package '{packageName}' is not installed"))
            {
                throw new Exception($"Package '{packageName}' is not installed!");
            }
            else if (!output.Contains($"Package: {packageName}\n"))
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
                    Version = cf.GetEntryContent("Version")
                };
            }
        }

        public IPlatformDependentPackageManager.PackageInstallationStatus CompareVersions(
            string versionA,
            string versionB
        )
        {
            var pva = new NativeVersion(versionA);
            var pvb = new NativeVersion(versionB);

            return pva.GetInstallationStatusFromComparison(pvb);
        }

        private async Task<(bool success, string logs)> ExecuteDpkg(
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

            var results = await cmd.WaitAndGetResultsAsync(); // wait for command to finish

            var output = await taskOutput; // wait for output to be fetched

            return (results.ExitCode == 0, String.Join(Environment.NewLine, output));
        }

        public Task<(bool success, string logs)> Install(string distroName, FileSystemPath filePath)
        {
            return ExecuteDpkg(
                distroName,
                "--force-confold",
                "--force-confdef",
                "-i",
                filePath.UnixPath
            );
        }

        public Task<(bool success, string logs)> Uninstall(string distroName, string packageName)
        {
            return ExecuteDpkg(distroName, "-r", packageName);
        }

        public Task<(bool success, string logs)> Upgrade(string distroName, FileSystemPath filePath)
        {
            return Install(distroName, filePath);
        }

        public Task<(bool success, string logs)> Downgrade(
            string distroName,
            FileSystemPath filePath
        )
        {
            return ExecuteDpkg(
                distroName,
                "--force-confold",
                "--force-confdef",
                "--force-downgrade",
                "-i",
                filePath.UnixPath
            );
        }

        public async Task<bool> IsSupportedByDistribution(string distroName)
        {
            var pathToDpkg = await _wslCommands.ExecuteCommandAsync(
                distroName,
                "which",
                new string[] { "dpkg" },
                ignoreExitCode: true
            );

            return pathToDpkg.Trim().Length > 0;
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
                        .Select((line) => (line ?? String.Empty).Replace("\0", ""))
                        .Where((line) => line.Length != 0)
            );
        }
    }
}
