using Community.Archives.Apk;
using Community.Archives.Core;
using Community.Wsa.Sdk.Strategies.Api;
using Community.Wsa.Sdk.Strategies.Packages;

namespace PackageInstaller.Core.Services;

public class AndroidPackageManager : IPlatformDependentPackageManager
{
    private readonly Community.Wsa.Sdk.Strategies.Packages.IPackageManager _packageManager;
    private readonly IWsaApi _wsaApi;

    public AndroidPackageManager(IPackageManager packageManager, IWsaApi _wsaApi)
    {
        _packageManager = packageManager;
        this._wsaApi = _wsaApi;
    }

    public Task<bool> IsPackageInstalledAsync(string distroName, string packageName)
    {
        return _packageManager.IsPackageInstalledAsync(packageName);
    }

    public async Task<IPlatformDependentPackageManager.PackageInfo> GetInstalledPackageInfoAsync(
        string distroName,
        string packageName
    )
    {
        var info = await _packageManager
            .GetInstalledPackageAsync(packageName)
            .ConfigureAwait(false);

        if (!info.HasValue)
        {
            throw new Exception($"The package {packageName} isn't installed.");
        }

        return new IPlatformDependentPackageManager.PackageInfo()
        {
            Name = info.Value.DisplayName, VersionCode = $"{info.Value.DisplayVersion} ({info.Value.VersionCode})"
        };
    }

    public async Task<IPlatformDependentPackageManager.PackageMetaData> ExtractPackageMetaDataAsync(
        FileSystemPath filePath
    )
    {
        var apkFileStream = File.OpenRead(filePath.WindowsPath);
        await using var _ = apkFileStream.ConfigureAwait(false);
        var apkReader = new ApkPackageReader();

        var md = await apkReader.GetMetaDataAsync(apkFileStream).ConfigureAwait(false);

        var iconFileNames = md.AllFields
            .GetValueOrDefault(ApkPackageReader.MANIFEST_ICON_FILE_NAMES_KEY, string.Empty)
            .Split(
                ApkPackageReader.MANIFEST_ARRAY_SEPARATOR,
                StringSplitOptions.RemoveEmptyEntries
            );

        var iconData = await FindLargestIconAsync(iconFileNames, apkFileStream).ConfigureAwait(false);

        return new IPlatformDependentPackageManager.PackageMetaData()
        {
            PackageName = md.Package,
            PackageLabel = md.AllFields.GetValueOrDefault(
                ApkPackageReader.MANIFEST_LABEL_KEY,
                md.Package
            ),
            Description = md.Description,
            VersionCode = md.AllFields.GetValueOrDefault(
                ApkPackageReader.MANIFEST_VERSION_CODE_KEY,
                String.Empty
            ),
            VersionLabel = md.Version,
            IconName = null,
            IconData = iconData,
            Architecture = md.Architecture,
            AllFields = md.AllFields
        };
    }

    private async Task<byte[]> FindLargestIconAsync(string[] iconFileNames, FileStream apkFileStream)
    {
        apkFileStream.Position = 0;

        var apk = new ApkPackageReader();

        byte[] largestFile = Array.Empty<byte>();

        await foreach (
            var archiveEntry in apk.GetFileEntriesAsync(
                    apkFileStream,
                    iconFileNames.Select((s) => $"^{s}$").ToArray()
                )
                .ConfigureAwait(false)
        )
        {
            if (archiveEntry.Content.Length > largestFile.Length)
            {
                largestFile = await archiveEntry.Content
                    .ReadBlockAsync(archiveEntry.Content.Length)
                    .ConfigureAwait(false);
            }
        }

        return largestFile;
    }

    public async Task<(bool isSupported, string? reason)> IsPackageSupportedAsync(
        FileSystemPath filePath
    )
    {
        return (filePath.WindowsPath.EndsWith(".apk"), String.Empty);
    }

    public IPlatformDependentPackageManager.PackageInstallationStatus CompareVersions(
        string baseVersion,
        string otherVersion
    )
    {
        if (
            !int.TryParse(baseVersion, out var intBaseVersion)
            || !int.TryParse(otherVersion, out var intOtherVersion)
        )
        {
            return IPlatformDependentPackageManager.PackageInstallationStatus.InstalledSameVersion;
        }

        if (intBaseVersion == intOtherVersion)
        {
            return IPlatformDependentPackageManager.PackageInstallationStatus.InstalledSameVersion;
        }
        else if (intBaseVersion < intOtherVersion)
        {
            return IPlatformDependentPackageManager.PackageInstallationStatus.InstalledOlderVersion;
        }
        else
        {
            return IPlatformDependentPackageManager.PackageInstallationStatus.InstalledNewerVersion;
        }
    }

    public async Task<(bool success, string logs)> InstallAsync(
        string distroName,
        FileSystemPath filePath
    )
    {
        try
        {
            await _wsaApi.EnsureWsaIsReadyAsync().ConfigureAwait(false);
            await _packageManager.InstallPackageAsync(filePath.WindowsPath).ConfigureAwait(false);

            return (true, String.Empty);
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
    }

    public async Task<(bool success, string logs)> UninstallAsync(
        string distroName,
        string packageName
    )
    {
        try
        {
            await _packageManager.UninstallPackageAsync(packageName).ConfigureAwait(false);

            return (true, String.Empty);
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
    }

    public Task<(bool success, string logs)> UpgradeAsync(
        string distroName,
        FileSystemPath filePath
    )
    {
        return InstallAsync(distroName, filePath);
    }

    public Task<(bool success, string logs)> DowngradeAsync(
        string distroName,
        FileSystemPath filePath
    )
    {
        return InstallAsync(distroName, filePath);
    }

    public Task<bool> IsSupportedByDistributionAsync(string distroName, string distroOrigin)
    {
        return Task.FromResult(distroOrigin == WsaProvider.ORIGIN_WSA);
    }
}
