using Community.Wsa.Sdk;
using Community.Wsa.Sdk.Exceptions;

namespace PackageInstaller.Core.Services;

public class WsaPackageManager : AndroidPackageManager
{
    private readonly IWsaApi _wsaApi;
    private readonly IWsaClient _wsaClient;
    private readonly Community.Wsa.Sdk.WsaPackageManager _wsaPackageManager;

    public WsaPackageManager(
        IWsaApi wsaApi,
        ILauncher launcher,
        IWsaClient wsaClient,
        IAdb adb,
        AdbPackageManager adbPackageManager,
        Community.Wsa.Sdk.WsaPackageManager wsaPackageManager
    ) : base(launcher, adb, adbPackageManager)
    {
        _wsaApi = wsaApi;
        _wsaClient = wsaClient;
        _wsaPackageManager = wsaPackageManager;
    }

    /// <summary>
    /// When <c>true</c>, <see cref="AdbPackageManager"/> is used for non-readonly operations (like installing or removing packages), instead of <see cref="Community.Wsa.Sdk.Strategies.Packages.WsaPackageManager"/>.
    /// </summary>
    public bool UseFallback { get; set; } = true;

    protected override IPackageManager GetPackageManager()
    {
        if (UseFallback)
        {
            return _adbPackageManager;
        }

        return _wsaPackageManager;
    }

    public override Task<bool> IsPackageInstalledAsync(string deviceId, string packageName)
    {
        return _wsaPackageManager.IsPackageInstalledAsync(deviceId, packageName);
    }

    public override async Task<IPlatformDependentPackageManager.PackageInfo> GetInstalledPackageInfoAsync(
        string deviceId,
        string packageName
    )
    {
        var info = await _wsaPackageManager
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

    public override async Task<(bool success, string logs)> InstallAsync(
        string deviceId,
        FileSystemPath filePath,
        IProgressController progressController
    )
    {
        await StartWsaServiceAsync(progressController).ConfigureAwait(false);

        return await base.InstallAsync(deviceId, filePath, progressController)
            .ConfigureAwait(false);
    }

    public override async Task<(bool success, string logs)> UninstallAsync(
        string deviceId,
        string packageName,
        IProgressController progressController
    )
    {
        await StartWsaServiceAsync(progressController).ConfigureAwait(false);

        return await base.UninstallAsync(deviceId, packageName, progressController)
            .ConfigureAwait(false);
    }

    public override async Task<(bool success, string logs)> UpgradeAsync(
        string deviceId,
        FileSystemPath filePath,
        IProgressController progressController
    )
    {
        await StartWsaServiceAsync(progressController).ConfigureAwait(false);

        return await base.UpgradeAsync(deviceId, filePath, progressController)
            .ConfigureAwait(false);
    }

    public override Task<bool> IsSupportedByDistributionAsync(string deviceId, string distroOrigin)
    {
        return Task.FromResult(distroOrigin == WsaProvider.ORIGIN_WSA);
    }

    public override async Task<(bool success, string logs)> DowngradeAsync(
        string deviceId,
        FileSystemPath filePath,
        IProgressController progressController
    )
    {
        await StartWsaServiceAsync(progressController).ConfigureAwait(false);

        return await base.DowngradeAsync(deviceId, filePath, progressController)
            .ConfigureAwait(false);
    }

    public override async Task LaunchAsync(
        string deviceId,
        string packageName,
        IProgressController progressController
    )
    {
        await StartWsaServiceAsync(progressController).ConfigureAwait(false);

        await base.LaunchAsync(deviceId, packageName, progressController).ConfigureAwait(false);
    }
}
