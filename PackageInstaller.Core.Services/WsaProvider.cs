using Community.Wsa.Sdk;

namespace PackageInstaller.Core.Services;

public class WsaProvider : IDistributionProvider
{
    public const string ORIGIN_WSA = "WSA";

    private IWsaApi _wsa;

    public WsaProvider(IWsaApi wsa)
    {
        _wsa = wsa;
    }

    public Task<DistributionList> GetAllInstalledDistributionsAsync(string packageExtensionHint)
    {
        var isApk = packageExtensionHint == "apk";

        if (isApk && !_wsa.IsWsaSupported(out var notSupportedMessage))
        {
            return Task.FromResult(
                DistributionList.CreateWithAlertOnly(
                    new DistributionList.Alert()
                    {
                        Message = notSupportedMessage ?? "WSA is not supported on this device.",
                        HelpUrl =
                            "https://support.microsoft.com/en-us/windows/install-mobile-apps-and-the-amazon-appstore-f8d0abb5-44ad-47d8-b9fb-ad6b1459ff6c",
                        Priority = DistributionList.AlertPriority.Critical
                    }
                )
            );
        }

        if (isApk && !_wsa.IsWsaInstalled)
        {
            return Task.FromResult(
                DistributionList.CreateWithAlertOnly(
                    new DistributionList.Alert()
                    {
                        Title = "WSA is not installed on this device.",
                        Message =
                            "Consider installing WSA if you want to manage android packages in WSA using this package manager.",
                        HelpUrl =
                            "https://docs.microsoft.com/en-us/windows/android/wsa/#install-the-amazon-appstore",
                        Priority = DistributionList.AlertPriority.Important
                    }
                )
            );
        }

        if (_wsa.IsWsaSupported() && _wsa.IsWsaInstalled)
        {
            return Task.FromResult(
                new DistributionList()
                {
                    InstalledDistributions = new Distribution[]
                    {
                        new Distribution()
                        {
                            IsAvailable = true,
                            IsRunning = true,
                            Version = new Version(11, 0),
                            Origin = ORIGIN_WSA,
                            Name = "Windows Subsystem for Android",
                            Id = IWsaApi.ADB_WSA_DEVICE_SERIAL_NUMBER,
                            Type = DistributionType.Android
                        }
                    }
                }
            );
        }

        return Task.FromResult(new DistributionList());
    }
}
