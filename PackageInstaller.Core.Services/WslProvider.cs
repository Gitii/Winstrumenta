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

    public Task<DistributionList> GetAllInstalledDistributionsAsync(string packageExtensionHint)
    {
        if (!_wsl.IsWslSupported(out var notSupportedMessage))
        {
            return Task.FromResult(
                DistributionList.CreateWithAlertOnly(
                    new DistributionList.Alert()
                    {
                        Message = notSupportedMessage ?? "WSL is not supported on this device.",
                        HelpUrl =
                            "https://docs.microsoft.com/en-us/windows/wsl/install#prerequisites",
                        Priority = DistributionList.AlertPriority.Critical
                    }
                )
            );
        }

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
                    Id = distroInfo.DistroName,
                    Origin = ORIGIN_WSL,
                    Version = new Version(distroInfo.WslVersion, 0),
                    Type = type,
                    IsAvailable = true
                }
            );
        }

        var alerts = GetAlerts(distributions);

        return Task.FromResult(
            new DistributionList() { Alerts = alerts, InstalledDistributions = distributions }
        );
    }

    private IReadOnlyList<DistributionList.Alert> GetAlerts(IList<Distribution> distributions)
    {
        if (distributions.Count == 0)
        {
            return new List<DistributionList.Alert>()
            {
                new DistributionList.Alert()
                {
                    Title = "There are no WSL distributions installed.",
                    Message =
                        "Consider installing one or more WSL distributions if you want to manage packages using this package manager.",
                    HelpUrl =
                        "https://docs.microsoft.com/en-us/windows/wsl/install#change-the-default-linux-distribution-installed",
                    Priority = DistributionList.AlertPriority.Important
                }
            };
        }

        if (distributions.Any((d) => !d.IsRunning))
        {
            return new List<DistributionList.Alert>()
            {
                new DistributionList.Alert()
                {
                    Title = "Not all WSL distributions are running.",
                    Message =
                        "At least one of the installed WSL distributions is not running. Please start the distribution if you want to manage packages in them.",
                    Priority = DistributionList.AlertPriority.Important
                }
            };
        }

        if (distributions.Any((d) => d.Version.Major < 2))
        {
            return new List<DistributionList.Alert>()
            {
                new DistributionList.Alert()
                {
                    Title = "Not all WSL distributions are running on WSL 2.",
                    Message =
                        "At least one of the installed WSL distributions is not running on WSL 2. "
                        + "Distributions running on WSL 1 are still displayed but not supported by this app. "
                        + "Consider upgrading them to WSL 2.",
                    Priority = DistributionList.AlertPriority.Important,
                    HelpUrl =
                        "https://docs.microsoft.com/en-us/windows/wsl/install#upgrade-version-from-wsl-1-to-wsl-2"
                }
            };
        }

        return Array.Empty<DistributionList.Alert>();
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
