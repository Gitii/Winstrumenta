using Community.Wsl.Sdk.Strategies.Api;

namespace PackageInstaller.Core.Services;

public class WslProvider : IDistributionProvider
{
    public const string ORIGIN_WSL = "WSL";

    private IWslApi _wsl;
    private IWslCommands _wslCommands;

    public WslProvider(IWslApi wsl, IWslCommands wslCommands)
    {
        _wsl = wsl;
        _wslCommands = wslCommands;
    }

    public Task<Distribution[]> GetAllInstalledDistributionsAsync()
    {
        var distros = _wsl.GetDistroList();

        List<Distribution> distributions = new List<Distribution>();

        foreach (var distroInfo in distros)
        {
            var type = GuessTypeFromName(distroInfo.DistroName);

            if (distroInfo.DistroName is ("docker-desktop-data" or "docker-desktop"))
            {
                continue;
            }

            distributions.Add(
                new Distribution()
                {
                    IsRunning = true,
                    Name = distroInfo.DistroName,
                    Origin = ORIGIN_WSL,
                    Version = distroInfo.WslVersion <= 1 ? new Version(1, 0) : new Version(2, 0),
                    Type = type,
                    IsAvailable = true
                }
            );
        }

        return Task.FromResult(distributions.ToArray());
    }

    private DistributionType GuessTypeFromName(string distroName)
    {
        foreach (var name in Enum.GetNames<DistributionType>())
        {
            if (distroName.Contains(name, StringComparison.Ordinal))
            {
                return Enum.Parse<DistributionType>(name);
            }
        }

        return DistributionType.Unknown;
    }
}
