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

    public Task<Distribution[]> GetAllInstalledDistributionsAsync()
    {
        if (_wsa.IsWsaSupported() && _wsa.IsWsaInstalled)
        {
            return Task.FromResult(
                new Distribution[]
                {
                    new Distribution()
                    {
                        IsAvailable = true,
                        IsRunning = true,
                        Version = new Version(1, 0),
                        Origin = ORIGIN_WSA,
                        Name = "Windows Subsystem for Android",
                        Id = IWsaApi.ADB_WSA_DEVICE_SERIAL_NUMBER,
                        Type = DistributionType.Android
                    }
                }
            );
        }

        return Task.FromResult(Array.Empty<Distribution>());
    }
}
