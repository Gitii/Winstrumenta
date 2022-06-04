using Community.Wsa.Sdk;

namespace PackageInstaller.Core.Services;

public class AndroidDeviceProvider : IDistributionProvider
{
    public const string ORIGIN_ANDROID_DEVICE = "ANDROID";

    private readonly IAdb _adb;

    public AndroidDeviceProvider(IAdb adb)
    {
        _adb = adb;
    }

    public async Task<Distribution[]> GetAllInstalledDistributionsAsync()
    {
        if (_adb.IsInstalled)
        {
            var devices = await _adb.ListDevicesAsync().ConfigureAwait(false);
            return await Task.WhenAll(devices.Where(NotWsaDevice).Select(ToDistributionAsync)).ConfigureAwait(false);
        }

        return Array.Empty<Distribution>();
    }

    private bool NotWsaDevice(KnownDevice kd)
    {
        return kd.ModelNumber != IWsaApi.WSA_MODEL_NUMBER;
    }

    private async Task<Distribution> ToDistributionAsync(KnownDevice kd)
    {
        var (deviceName, androidVersion) = await IdentifyDeviceAsync(kd).ConfigureAwait(false);

        return new Distribution()
        {
            Id = kd.DeviceSerialNumber,
            IsAvailable = !kd.IsOffline,
            IsRunning = !kd.IsOffline,
            Origin = ORIGIN_ANDROID_DEVICE,
            Type = DistributionType.Android,
            Name = deviceName,
            Version = androidVersion
        };
    }

    private async Task<(string deviceName, Version androidVersion)> IdentifyDeviceAsync(KnownDevice kd)
    {
        var deviceName = await _adb.ExecuteCommandAsync("getprop", new string[] { "ro.product.model" })
            .ConfigureAwait(false);
        var androidVersion = await _adb.ExecuteCommandAsync("getprop", new string[] { "ro.build.version.release" })
            .ConfigureAwait(false);

        var version = ParseVersion(androidVersion);

        return (deviceName, version);
    }

    private Version ParseVersion(string versionString)
    {
        var parts = versionString.Split(".").Select(int.Parse).ToArray();
        return parts.Length switch
        {
            0 => new Version(0, 0),
            1 => new Version(parts[0], 0),
            2 => new Version(parts[0], parts[1]),
            _ => new Version(parts[0], parts[1], parts[2]),
        };
    }
}
