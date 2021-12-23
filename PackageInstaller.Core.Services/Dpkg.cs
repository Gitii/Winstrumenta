using System.Reactive.Linq;
using Community.Wsl.Sdk;
using ReactiveUI;

namespace PackageInstaller.Core.Services
{
    public class Dpkg : IDpkg
    {
        private IWslCommands _wslCommands;

        public Dpkg(IWslCommands wslCommands)
        {
            _wslCommands = wslCommands;
        }

        public async Task<bool> IsPackageInstalled(string distroName, string packageName)
        {
            var output = await _wslCommands
                .ExecuteCommandAsync(
                    distroName,
                    "dpkg",
                    new[] { "--status-logger=echo", "1", "-s", packageName }
                )
                .ConfigureAwait(false);

            return !output.Contains($"dpkg-query: package '{packageName}' is not installed");
        }

        public async Task<IDpkg.PackageInfo> GetPackage(string distroName, string packageName)
        {
            var output = await _wslCommands
                .ExecuteCommandAsync(
                    distroName,
                    "dpkg",
                    new[] { "--status-logger=echo", "1", "-s", packageName }
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

        public int CompareVersions(string versionA, string versionB)
        {
            var pva = new NativeVersion(versionA);
            var pvb = new NativeVersion(versionB);

            return pva.CompareTo(pvb);
        }

        public async Task Install(
            string distroName,
            string unixfilePath,
            IProgress<(int, string)> progress
        )
        {
            var cmd = _wslCommands.CreateCommand(
                distroName,
                "dpkg",
                new[] { "--status-fd=echo", "-i", unixfilePath },
                new CommandExecutionOptions()
                {
                    FailOnNegativeExitCode = false,
                    StdErrDataProcessingMode = DataProcessingMode.External,
                    StdoutDataProcessingMode = DataProcessingMode.External
                },
                true
            );

            var pipes = cmd.Start();

            ReadLines(pipes.StandardOutput).ObserveOn(RxApp.TaskpoolScheduler)
                .Select(ParseStatusMessage)

            var results = await cmd.WaitAndGetResultsAsync();
        }

        private DpkgStatusMessage? ParseStatusMessage<TResult>(string line)
        {
            var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length <= 3)
            {
                return null;
            }

            if (parts[0] == "status")
        }

        private readonly struct DpkgStatusMessage
        {
            public bool IsStatusUpdate { get; init; }

            public bool IsProgressUpdate { get; init; }

            public string? Package { get; init; }

            public string? Error { get; init; }

            public string ProgressState { get; init; }
        }

        public static IObservable<string> ReadLines(StreamReader reader)
        {
            return Observable.Using(
                () => reader,
                r => Observable.FromAsync(r.ReadLineAsync)
                    .Repeat()
                    .TakeWhile(line => line != null).Select((line) => line ?? String.Empty));
        }
    }
}
