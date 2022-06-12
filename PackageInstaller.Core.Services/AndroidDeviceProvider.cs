using Community.Wsa.Sdk;

namespace PackageInstaller.Core.Services;

public class AndroidDeviceProvider : IDistributionProvider
{
    public const string ORIGIN_ANDROID_DEVICE = "ANDROID";
    public const string APK_FILE_EXTENSION = "apk";

    private readonly IAdb _adb;

    public AndroidDeviceProvider(IAdb adb)
    {
        _adb = adb;
    }

    public async Task<DistributionList> GetAllInstalledDistributionsAsync(
        string packageExtensionHint
    )
    {
        var isApk = packageExtensionHint == APK_FILE_EXTENSION;

        if (_adb.IsInstalled)
        {
            var devices = await _adb.ListDevicesAsync().ConfigureAwait(false);

            if (isApk && devices.Length == 0)
            {
                return DistributionList.CreateWithAlertOnly(
                    new DistributionList.Alert()
                    {
                        Title = "No android devices are connected.",
                        Message =
                            "There are no android devices connected to this device. "
                            + "If you want to manage android packages on physical devices or emulators using this package manager, "
                            + "consider to connect your android devices and enable ADB in the developer options.",
                        Priority = DistributionList.AlertPriority.Important,
                        HelpUrl = "https://developer.android.com/studio/command-line/adb#Enabling",
                    }
                );
            }

            var distros = await Task.WhenAll(
                    devices.Where(NotWsaDevice).Select(ToDistributionAsync)
                )
                .ConfigureAwait(false);

            return new DistributionList() { InstalledDistributions = distros };
        }

        if (isApk)
        {
            return DistributionList.CreateWithAlertOnly(
                new DistributionList.Alert()
                {
                    Title = "Android Debug Bridge (adb) is missing.",
                    Message =
                        "The Android Debug Bridge (adb) could not be found on this device. Please install or it and add it to your PATH environment variable.",
                    Priority = DistributionList.AlertPriority.Critical,
                    HelpUrl = "https://developer.android.com/studio/command-line/adb",
                }
            );
        }

        return new DistributionList();
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

    private async Task<(string deviceName, Version androidVersion)> IdentifyDeviceAsync(
        KnownDevice kd
    )
    {
        var deviceName = await _adb.ExecuteCommandAsync(
                "getprop",
                new string[] { "ro.product.model" }
            )
            .ConfigureAwait(false);
        var androidVersion = await _adb.ExecuteCommandAsync(
                "getprop",
                new string[] { "ro.build.version.release" }
            )
            .ConfigureAwait(false);

        return (deviceName, ParseVersion(androidVersion));
    }

    private Version ParseVersion(string versionString)
    {
        var parts = versionString
            .Split(".", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToArray();
        return parts.Length switch
        {
            0 => new Version(0, 0),
            1 => new Version(parts[0], 0),
            2 => new Version(parts[0], parts[1]),
            _ => new Version(parts[0], parts[1], parts[2]),
        };
    }
}
