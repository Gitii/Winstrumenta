using Community.Wsl.Sdk;
using Community.Wsl.Sdk.Strategies.Api;

namespace PackageInstaller.Core.Services
{
    public class WslImpl : IWsl
    {
        private bool _securityInitialized;

        private IWslApi _wsl;
        private IWslCommands _wslCommands;

        public WslImpl(IWslApi wsl, IWslCommands wslCommands)
        {
            _wsl = wsl;
            _wslCommands = wslCommands;
        }

        public void AssertWslIsReady()
        {
            if (!_securityInitialized)
            {
                _securityInitialized = true;
                _wsl.InitializeSecurityModel();
            }

            if (!_wsl.IsWslSupported(out string? reason))
            {
                throw new PlatformNotSupportedException(reason);
            }
        }

        public async Task<WslDistribution[]> GetAllInstalledDistributions()
        {
            var distros = _wsl.GetDistroList();

            List<WslDistribution> distributions = new List<WslDistribution>();

            foreach (var distroInfo in distros)
            {
                var supportedPackageTypes = await GetSupportedPackageTypes(distroInfo);
                var type = GuessTypeFromName(distroInfo.DistroName);

                if (type == WslDistributionType.Unknown)
                {
                    if (Enum.IsDefined(PackageTypes.Deb))
                    {
                        type = WslDistributionType.GeneralDebBased;
                    }
                    else if (Enum.IsDefined(PackageTypes.Rpm))
                    {
                        type = WslDistributionType.GeneralRpmBased;
                    }
                }

                distributions.Add(
                    new WslDistribution()
                    {
                        IsDefault = distroInfo.IsDefault,
                        IsRunning = true,
                        Name = distroInfo.DistroName,
                        Version =
                            distroInfo.WslVersion <= 1
                                ? WslDistributionVersion.One
                                : WslDistributionVersion.Two,
                        Type = type,
                        SupportedPackageTypes = supportedPackageTypes
                    }
                );
            }

            return distributions.ToArray();
        }

        private async Task<PackageTypes> GetSupportedPackageTypes(DistroInfo distroInfo)
        {
            PackageTypes supportedTypes = PackageTypes.Unknown;
            if (await RunSimpleWslCommand("which", "dpkg") == 0)
            {
                supportedTypes &= ~PackageTypes.Unknown;
                supportedTypes |= PackageTypes.Deb;
            }

            if (
                await RunSimpleWslCommand("which", "rpm") == 0
                || await RunSimpleWslCommand("which", "yum") == 0
            )
            {
                supportedTypes &= ~PackageTypes.Unknown;
                supportedTypes |= PackageTypes.Rpm;
            }

            return supportedTypes;

            async Task<int> RunSimpleWslCommand(string command, params string[] arguments)
            {
                var cmd = _wslCommands.CreateCommand(
                    distroInfo.DistroName,
                    command,
                    arguments,
                    new CommandExecutionOptions() { FailOnNegativeExitCode = false }
                );

                return (await cmd.StartAndGetResultsAsync()).ExitCode;
            }
        }

        private WslDistributionType GuessTypeFromName(string distroName)
        {
            foreach (var name in Enum.GetNames<WslDistributionType>())
            {
                if (distroName.Contains(name))
                {
                    return Enum.Parse<WslDistributionType>(name);
                }
            }

            return WslDistributionType.Unknown;
        }

        public async Task<int> ExecuteCommand(
            WslDistribution distribution,
            string command,
            string[] arguments,
            Action<string> onDataReceived
        )
        {
            var context = SynchronizationContext.Current!;

            _wsl.InitializeSecurityModel();

            var cmd = _wslCommands.CreateCommand(
                distribution.Name,
                command,
                arguments,
                new CommandExecutionOptions()
                {
                    FailOnNegativeExitCode = false,
                    StdErrDataProcessingMode = DataProcessingMode.Drop,
                    StdInDataProcessingMode = DataProcessingMode.External
                }
            );

            var streams = cmd.Start();

            _ = Task.Run(
                () =>
                {
                    var reader = streams.StandardOutput;

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line != null)
                        {
                            OnDataReceivedSync(line);
                        }
                    }
                }
            );

            var result = await cmd.WaitAndGetResultsAsync();

            return result.ExitCode;

            void PostCallback(object? objData) => onDataReceived((string)objData!);
            void OnDataReceivedSync(string data) => context.Post(PostCallback, data);
        }
    }
}
