using Community.Archives.Apk;
using Community.Archives.Core;
using Community.Wsa.Sdk;
using Community.Wsa.Sdk.Exceptions;

namespace PackageInstaller.Core.Services;

public class AndroidPackageManager : IPlatformDependentPackageManager
{
    protected readonly AdbPackageManager _adbPackageManager;
    protected readonly ILauncher _launcher;
    protected readonly IAdb _adb;

    public AndroidPackageManager(ILauncher launcher, IAdb adb, AdbPackageManager adbPackageManager)
    {
        _launcher = launcher;
        _adb = adb;
        _adbPackageManager = adbPackageManager;
    }

    protected virtual IPackageManager GetPackageManager()
    {
        return _adbPackageManager;
    }

    public virtual Task<bool> IsPackageInstalledAsync(string deviceId, string packageName)
    {
        return _adbPackageManager.IsPackageInstalledAsync(deviceId, packageName);
    }

    public virtual async Task<IPlatformDependentPackageManager.PackageInfo> GetInstalledPackageInfoAsync(
        string deviceId,
        string packageName
    )
    {
        var info = await _adbPackageManager
            .GetInstalledPackageAsync(deviceId, packageName)
            .ConfigureAwait(false);

        if (!info.HasValue)
        {
            throw new Exception($"The package {packageName} isn't installed.");
        }

        return new IPlatformDependentPackageManager.PackageInfo()
        {
            Name = info.Value.DisplayName,
            VersionCode = $"{info.Value.DisplayVersion} ({info.Value.VersionCode})"
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

        var iconData = await FindLargestIconAsync(iconFileNames, apkFileStream)
            .ConfigureAwait(false);

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

    private async Task<byte[]> FindLargestIconAsync(
        string[] iconFileNames,
        FileStream apkFileStream
    )
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

    public virtual async Task<(bool success, string logs)> InstallAsync(
        string deviceId,
        FileSystemPath filePath,
        IProgressController progressController
    )
    {
        await ExecuteActionAsync(
                deviceId,
                progressController,
                (pm, id) =>
                    pm.InstallPackageAsync(
                        id,
                        filePath.WindowsPath,
                        progressController.CreateStatusReporter()
                    ),
                "Installing application...",
                "Failed to install package"
            )
            .ConfigureAwait(false);

        return (true, String.Empty);
    }

    private async Task ExecuteActionAsync(
        string deviceId,
        IProgressController progressController,
        Func<IPackageManager, string, Task> action,
        string operationTitle,
        string genericErrorMessage
    )
    {
        progressController.StartNew(operationTitle, true);
        try
        {
            await action(GetPackageManager(), deviceId).ConfigureAwait(false);
        }
        catch (AdbException adbException)
        {
            progressController.Fail(adbException.Message, DetermineBestActionFrom(adbException));

            throw;
        }
        catch (Exception e)
        {
            progressController.Fail(genericErrorMessage + "\n\n" + e.Message);

            throw;
        }
    }

    protected virtual RecoveryAction[] DetermineBestActionFrom(AdbException adbException)
    {
        switch (adbException.Error)
        {
            case AdbError.AdbIsNotInstalled:
                return new RecoveryAction[] { DownloadPlatformToolsAction() };
            case AdbError.CannotStartAdb:
                if (!_adb.IsInstalled)
                {
                    return new[] { DownloadPlatformToolsAction() };
                }

                var path = _adb.PathToAdb;
                if (!string.IsNullOrEmpty(path))
                {
                    return new[] { CheckAdbInExplorer(path) };
                }

                return new[] { DownloadPlatformToolsAction() };
            case AdbError.CommandFailed:
                return Array.Empty<RecoveryAction>();
            case AdbError.CommandTimedOut:
                return Array.Empty<RecoveryAction>();
            default:
                throw new ArgumentOutOfRangeException();
        }

        RecoveryAction DownloadPlatformToolsAction()
        {
            return new RecoveryAction()
            {
                InstructionText = "Download platform tools",
                InstructionDetails = "and install them on your System",
                OpensExternalProgram = true,
                Action = () =>
                    _launcher.LaunchAsync(
                        new Uri("https://developer.android.com/studio/releases/platform-tools")
                    ),
            };
        }

        RecoveryAction CheckAdbInExplorer(string adbPath)
        {
            return new RecoveryAction()
            {
                InstructionText = "Open Windows Explorer",
                InstructionDetails = "and check the adb executable",
                OpensExternalProgram = true,
                Action = () => _launcher.LaunchFolderAsync(adbPath)
            };
        }
    }

    public virtual async Task<(bool success, string logs)> UninstallAsync(
        string deviceId,
        string packageName,
        IProgressController progressController
    )
    {
        await ExecuteActionAsync(
                deviceId,
                progressController,
                (pm, id) => pm.UninstallPackageAsync(id, packageName),
                "Uninstalling package",
                "Failed to uninstall package"
            )
            .ConfigureAwait(false);

        return (true, String.Empty);
    }

    public virtual async Task<(bool success, string logs)> UpgradeAsync(
        string deviceId,
        FileSystemPath filePath,
        IProgressController progressController
    )
    {
        await ExecuteActionAsync(
                deviceId,
                progressController,
                (pm, id) => pm.InstallPackageAsync(id, filePath.WindowsPath),
                "Upgrading application...",
                "Failed to upgrade application"
            )
            .ConfigureAwait(false);

        return (true, String.Empty);
    }

    public virtual async Task<(bool success, string logs)> DowngradeAsync(
        string deviceId,
        FileSystemPath filePath,
        IProgressController progressController
    )
    {
        await ExecuteActionAsync(
                deviceId,
                progressController,
                (pm, id) => pm.InstallPackageAsync(id, filePath.WindowsPath),
                "Downgrading application...",
                "Failed to downgrade application"
            )
            .ConfigureAwait(false);

        return (true, String.Empty);
    }

    public virtual Task<bool> IsSupportedByDistributionAsync(string deviceId, string distroOrigin)
    {
        return Task.FromResult(distroOrigin == AndroidDeviceProvider.ORIGIN_ANDROID_DEVICE);
    }

    public virtual Task LaunchAsync(
        string deviceId,
        string packageName,
        IProgressController progressController
    )
    {
        progressController.StartNew("Launching package...", true);

        return GetPackageManager().LaunchAsync(deviceId, packageName);
    }
}
