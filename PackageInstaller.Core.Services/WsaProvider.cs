using Community.Wsa.Sdk;

namespace PackageInstaller.Core.Services;

public class WsaProvider : IDistributionProvider
{
    public const string ORIGIN_WSA = "WSA";
    public const string ANDROID_PACKAGE_EXTENSION = "apk";

    private IWsaApi _wsa;

    public WsaProvider(IWsaApi wsa)
    {
        _wsa = wsa;
    }

    public Task<DistributionList> GetAllInstalledDistributionsAsync(string packageExtensionHint)
    {
        var isApk = packageExtensionHint == ANDROID_PACKAGE_EXTENSION;

        string? notSupportedMessage;
        var isWsaSupported = _wsa.IsWsaSupported(out notSupportedMessage);
        var isWsaInstalled = _wsa.IsWsaInstalled;

        if (isApk && !isWsaSupported)
        {
            return CreateWsaNotSupportedResultAsync(notSupportedMessage);
        }

        if (isApk && isWsaSupported && !isWsaInstalled)
        {
            return CreateWsaNotInstalledResultAsync();
        }

        if (isWsaSupported && isWsaInstalled)
        {
            return CreateWsaDeviceListResultAsync();
        }

        return Task.FromResult(new DistributionList());
    }

    private static Task<DistributionList> CreateWsaNotSupportedResultAsync(
        string? notSupportedMessage
    )
    {
        return Task.FromResult(
            DistributionList.CreateWithAlertOnly(
                new DistributionList.Alert()
                {
                    Title = "WSA is not supported on this device.",
                    Message = notSupportedMessage ?? "WSA is not supported on this device.",
                    HelpUrl =
                        "https://support.microsoft.com/en-us/windows/install-mobile-apps-and-the-amazon-appstore-f8d0abb5-44ad-47d8-b9fb-ad6b1459ff6c",
                    Priority = DistributionList.AlertPriority.Critical
                }
            )
        );
    }

    private static Task<DistributionList> CreateWsaNotInstalledResultAsync()
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

    private static Task<DistributionList> CreateWsaDeviceListResultAsync()
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
}
