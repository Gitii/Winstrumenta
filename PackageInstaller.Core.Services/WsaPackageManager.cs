using Community.Archives.Apk;
using Community.Archives.Core;
using Community.Wsa.Sdk;
using Community.Wsa.Sdk.Exceptions;

namespace PackageInstaller.Core.Services;

public class WsaPackageManager : IPlatformDependentPackageManager
{
    private readonly AdbPackageManager _adbPackageManager;
    private readonly Community.Wsa.Sdk.WsaPackageManager _wsaPackageManager;
    private readonly IWsaApi _wsaApi;
    private readonly ILauncher _launcher;
    private readonly IWsaClient _wsaClient;
    private readonly IAdb _adb;

    public WsaPackageManager(
        IWsaApi wsaApi,
        ILauncher launcher,
        IWsaClient wsaClient,
        IAdb adb,
        AdbPackageManager adbPackageManager,
        Community.Wsa.Sdk.WsaPackageManager wsaPackageManager
    )
    {
        _wsaApi = wsaApi;
        _launcher = launcher;
        _wsaClient = wsaClient;
        _adb = adb;
        _adbPackageManager = adbPackageManager;
        _wsaPackageManager = wsaPackageManager;
    }

    /// <summary>
    /// When <c>true</c>, <see cref="AdbPackageManager"/> is used for non-readonly operations (like installing or removing packages), instead of <see cref="Community.Wsa.Sdk.Strategies.Packages.WsaPackageManager"/>.
    /// </summary>
    public bool UseFallback { get; set; } = true;

    private IPackageManager GetPackageManager()
    {
        if (UseFallback)
        {
            return _adbPackageManager;
        }

        return _wsaPackageManager;
    }

    public Task<bool> IsPackageInstalledAsync(string distroName, string packageName)
    {
        return _wsaPackageManager.IsPackageInstalledAsync(distroName, packageName);
    }

    public async Task<IPlatformDependentPackageManager.PackageInfo> GetInstalledPackageInfoAsync(
        string distroName,
        string packageName
    )
    {
        var info = await _wsaPackageManager
            .GetInstalledPackageAsync(distroName, packageName)
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

    public async Task<(bool success, string logs)> InstallAsync(
        string distroName,
        FileSystemPath filePath,
        IProgressController progressController
    )
    {
        await StartWsaServiceAsync(progressController).ConfigureAwait(false);

        await ExecuteActionAsync(
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
        IProgressController progressController,
        Func<IPackageManager, string, Task> action,
        string operationTitle,
        string genericErrorMessage
    )
    {
        progressController.StartNew(operationTitle, true);
        try
        {
            var deviceId = await GetWsaDeviceIdAsync().ConfigureAwait(false);

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

    private async Task<string> GetWsaDeviceIdAsync()
    {
        var devices = await _adb.ListDevicesAsync().ConfigureAwait(false);
        var wsaDevice = devices.FirstOrNullable((d) => d.ModelNumber == IWsaApi.WSA_MODEL_NUMBER);
        if (wsaDevice.HasValue)
        {
            return wsaDevice.Value.DeviceSerialNumber;
        }

        throw new ServiceException(ServiceError.CannotConnectToDevice);
    }

    private async Task StartWsaServiceAsync(IProgressController progressController)
    {
        progressController.StartNew("Starting Windows Subsystem for Android...", true);
        try
        {
            await _wsaApi
                .EnsureWsaIsReadyAsync(progressController.CreateStatusReporter())
                .ConfigureAwait(false);
        }
        catch (ServiceException serviceException)
        {
            progressController.Fail(
                serviceException.Message,
                DetermineBestActionFrom(serviceException)
            );

            throw;
        }
        catch (Exception e)
        {
            progressController.Fail(e.Message);

            throw;
        }
    }

    private RecoveryAction[] DetermineBestActionFrom(AdbException adbException)
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

    private RecoveryAction[] DetermineBestActionFrom(ServiceException serviceException)
    {
        switch (serviceException.Error)
        {
            case ServiceError.CannotStartService:
                return new[]
                {
                    new RecoveryAction()
                    {
                        InstructionText = "Open WSA Settings",
                        InstructionDetails = "and open Files app to start WSA manually",
                        OpensExternalProgram = true,
                        Action = _wsaClient.LaunchWsaSettingsAsync,
                    }
                };
            case ServiceError.CannotConnectToService:
            case ServiceError.CannotConnectToDevice:
                return new[]
                {
                    new RecoveryAction()
                    {
                        InstructionText = "Open WSA Settings",
                        InstructionDetails = "and enable Developer Mode",
                        OpensExternalProgram = true,
                        Action = _wsaClient.LaunchWsaSettingsAsync,
                    }
                };
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async Task<(bool success, string logs)> UninstallAsync(
        string distroName,
        string packageName,
        IProgressController progressController
    )
    {
        await StartWsaServiceAsync(progressController).ConfigureAwait(false);

        await ExecuteActionAsync(
                progressController,
                (pm, id) => pm.UninstallPackageAsync(id, packageName),
                "Uninstalling package",
                "Failed to uninstall package"
            )
            .ConfigureAwait(false);

        return (true, String.Empty);
    }

    public async Task<(bool success, string logs)> UpgradeAsync(
        string distroName,
        FileSystemPath filePath,
        IProgressController progressController
    )
    {
        await StartWsaServiceAsync(progressController).ConfigureAwait(false);

        await ExecuteActionAsync(
                progressController,
                (pm, id) => pm.InstallPackageAsync(id, filePath.WindowsPath),
                "Upgrading application...",
                "Failed to upgrade application"
            )
            .ConfigureAwait(false);

        return (true, String.Empty);
    }

    public async Task<(bool success, string logs)> DowngradeAsync(
        string distroName,
        FileSystemPath filePath,
        IProgressController progressController
    )
    {
        await StartWsaServiceAsync(progressController).ConfigureAwait(false);

        await ExecuteActionAsync(
                progressController,
                (pm, id) => pm.InstallPackageAsync(id, filePath.WindowsPath),
                "Downgrading application...",
                "Failed to downgrade application"
            )
            .ConfigureAwait(false);

        return (true, String.Empty);
    }

    public Task<bool> IsSupportedByDistributionAsync(string distroName, string distroOrigin)
    {
        return Task.FromResult(distroOrigin == WsaProvider.ORIGIN_WSA);
    }

    public Task LaunchAsync(string distroName, string packageName)
    {
        return GetPackageManager().LaunchAsync(distroName, packageName);
    }
}
