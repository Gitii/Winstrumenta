using Community.Wsl.Sdk.Strategies.Api;

namespace PackageInstaller.Core.Services
{
    public class WslImpl : IWsl
    {
        private IWslApi _wsl;
        private IWslCommands _wslCommands;

        public WslImpl(IWslApi wsl, IWslCommands wslCommands)
        {
            _wsl = wsl;
            _wslCommands = wslCommands;
        }

        public async Task<WslDistribution[]> GetAllInstalledDistributions()
        {
            var distros = _wsl.GetDistroList();

            List<WslDistribution> distributions = new List<WslDistribution>();

            foreach (var distroInfo in distros)
            {
                var type = GuessTypeFromName(distroInfo.DistroName);

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
                        Type = type
                    }
                );
            }

            return distributions.ToArray();
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
    }
}
