using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using Microsoft.Extensions.Hosting;
using PackageInstaller.Core.Exceptions;
using PackageInstaller.Core.Helpers;
using PackageInstaller.Core.Services;
using ReactiveUI;
using Sextant;

namespace PackageInstaller.Core.ModelViews;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "MA0004:Use Task.ConfigureAwait(false)",
    Justification = "ModelView should care about thread context."
)]
public class PackageActionsViewModel : ReactiveObject, IViewModel, INavigable
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IList<IDistributionProvider> _distributionProviders;
    private readonly SourceList<WslDistributionModelView> _distroSourceList;
    private readonly ReadOnlyObservableCollection<WslDistributionModelView> _distroList;
    private IEnumerable<IPlatformDependentPackageManager> _packageManagers;
    bool _inProgress;
    string _progressStatusMessage;
    FileSystemPath? _packageFilePath;
    private readonly IParameterViewStackService _viewStackService;
    private IPath _path;
    Stream? _packageIconStream;
    private IThreadHelpers _threadHelpers;
    private IIconThemeManager _iconThemeManager;

    public Stream? PackageIconStream
    {
        get { return _packageIconStream; }
        set { this.RaiseAndSetIfChanged(ref _packageIconStream, value); }
    }

    public FileSystemPath? PackageFilePath
    {
        get { return _packageFilePath; }
        set { this.RaiseAndSetIfChanged(ref _packageFilePath, value); }
    }

    public string ProgressStatusMessage
    {
        get { return _progressStatusMessage; }
        set { this.RaiseAndSetIfChanged(ref _progressStatusMessage, value); }
    }

    public bool InProgress
    {
        get { return _inProgress; }
        private set { this.RaiseAndSetIfChanged(ref _inProgress, value); }
    }

    public readonly struct NavigationParameter
    {
        public readonly IPlatformDependentPackageManager.PackageMetaData PackageMetaData { get; init; }

        public readonly FileSystemPath PackageFilePath { get; init; }
    }

    public PackageActionsViewModel(
        IHostApplicationLifetime applicationLifetime,
        IEnumerable<IDistributionProvider> distributionProviders,
        IParameterViewStackService viewStackService,
        IEnumerable<IPlatformDependentPackageManager> packageManagers,
        IPath path,
        IIconThemeManager iconThemeManager,
        IThreadHelpers threadHelpers
    )
    {
        _applicationLifetime = applicationLifetime;
        _distributionProviders = distributionProviders.ToList();
        _viewStackService = viewStackService;
        _packageManagers = packageManagers;
        _path = path;
        _iconThemeManager = iconThemeManager;
        _threadHelpers = threadHelpers;

        _progressStatusMessage = String.Empty;
        ProgressStatusMessage = String.Empty;

        _distroSourceList = new SourceList<WslDistributionModelView>();
        _distroSourceList
            .Connect()
            .AutoRefresh()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _distroList)
            .DisposeMany()
            .Subscribe();

        Close = ReactiveCommand.Create(
            () =>
            {
                _applicationLifetime.StopApplication();
                Environment.Exit(0);
            }
        );

        this.WhenAnyValue((vm) => vm.SelectedWslDistribution)
            .ObserveOn(RxApp.MainThreadScheduler)
            .SelectMany(OnSelectedDistributionChangedAsync)
            .Subscribe();

        PrimaryPackageCommand = ReactiveCommand.CreateFromTask(ExecutePrimaryCommandAsync);

        InstallCommand = ReactiveCommand.CreateFromTask(InstallAsync);
        ReInstallCommand = ReactiveCommand.CreateFromTask(ReInstallAsync);
        UpgradeCommand = ReactiveCommand.CreateFromTask(UpgradeAsync);
        DowngradeCommand = ReactiveCommand.CreateFromTask(DowngradeAsync);
        UninstallCommand = ReactiveCommand.CreateFromTask(UninstallAsync);
    }

    private Task UninstallAsync()
    {
        return Task.CompletedTask;
    }

    private Task ExecutePrimaryCommandAsync()
    {
        switch (PackageInstallationStatus)
        {
            case IPlatformDependentPackageManager.PackageInstallationStatus.NotInstalled:
                return InstallAsync();
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledSameVersion:
                return ReInstallAsync();
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledOlderVersion:
                return UpgradeAsync();
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledNewerVersion:
                return DowngradeAsync();
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(PackageInstallationStatus),
                    PackageInstallationStatus?.ToString()
                );
        }
    }

    private async Task DowngradeAsync()
    {
        try
        {
            InProgress = true;
            ProgressStatusMessage = String.Empty;

            ProgressStatusMessage = "Downgrading package...";

            var distroName = SelectedWslDistribution!.Name;

            var packageManager = await _packageManagers.GetSupportedManagerAsync(
                PackageFilePath ?? throw new Exception("Package file path is null"),
                distroName,
                SelectedWslDistribution.Distro.Origin
            );

            var (success, log) = await packageManager.DowngradeAsync(distroName, PackageFilePath);

            if (!success)
            {
                throw new DetailedException("Failed to install package", log);
            }

            ProgressStatusMessage = "Package downgraded!";

            var navParms = new ResultViewModel.NavigationParameter()
            {
                Title = "Package has been downgraded successfully",
                Description =
                    $"The package '{PackageMetaData.PackageLabel}' has been downgraded to '{PackageMetaData.VersionLabel}' in '{SelectedWslDistribution.Name}'.",
                Details = log
            };

            _viewStackService
                .PushPage<ResultViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
        catch (Exception e)
        {
            var navParms = new ErrorViewModel.NavigationParameter() { Exception = e };

            _viewStackService
                .PushPage<ErrorViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
        finally
        {
            InProgress = false;
            ProgressStatusMessage = String.Empty;
        }
    }

    private async Task UpgradeAsync()
    {
        try
        {
            InProgress = true;
            ProgressStatusMessage = String.Empty;

            ProgressStatusMessage = "Upgrading package...";

            var distroName = SelectedWslDistribution!.Name;

            var packageManager = await _packageManagers.GetSupportedManagerAsync(
                PackageFilePath ?? throw new Exception("Package file path is null"),
                distroName,
                SelectedWslDistribution.Distro.Origin
            );

            var (success, log) = await packageManager.UpgradeAsync(distroName, PackageFilePath);

            if (!success)
            {
                throw new DetailedException("Failed to install package", log);
            }

            ProgressStatusMessage = "Package upgraded!";

            var navParms = new ResultViewModel.NavigationParameter()
            {
                Title = "Package has been upgraded successfully",
                Description =
                    $"The package '{PackageMetaData.PackageLabel}' has been upgraded to '{PackageMetaData.VersionLabel}' in '{SelectedWslDistribution.Name}'.",
                Details = log
            };

            _viewStackService
                .PushPage<ResultViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
        catch (Exception e)
        {
            var navParms = new ErrorViewModel.NavigationParameter() { Exception = e };

            _viewStackService
                .PushPage<ErrorViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
        finally
        {
            InProgress = false;
            ProgressStatusMessage = String.Empty;
        }
    }

    private Task ReInstallAsync()
    {
        return InstallAsync();
    }

    private async Task InstallAsync()
    {
        try
        {
            InProgress = true;
            ProgressStatusMessage = String.Empty;

            ProgressStatusMessage = "Installing package...";

            var distroName = SelectedWslDistribution!.Name;

            var packageManager = await _packageManagers.GetSupportedManagerAsync(
                PackageFilePath ?? throw new Exception("Package file path is null"),
                distroName,
                SelectedWslDistribution.Distro.Origin
            );

            var (success, log) = await packageManager.InstallAsync(distroName, PackageFilePath);

            if (!success)
            {
                throw new DetailedException("Failed to install package", log);
            }

            ProgressStatusMessage = "Package installed!";

            var navParms = new ResultViewModel.NavigationParameter()
            {
                Title = "Package has been installed successfully",
                Description =
                    $"The package '{PackageMetaData.PackageLabel}' has been installed in '{SelectedWslDistribution.Name}'.",
                Details = log
            };

            _viewStackService
                .PushPage<ResultViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
        catch (Exception e)
        {
            var navParms = new ErrorViewModel.NavigationParameter() { Exception = e };

            _viewStackService
                .PushPage<ErrorViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
        finally
        {
            InProgress = false;
            ProgressStatusMessage = String.Empty;
        }
    }

    private async Task<WslDistributionModelView?> OnSelectedDistributionChangedAsync(
        WslDistributionModelView? arg
    )
    {
        if (arg != null)
        {
            var distroName = arg.Name;

            var packageManager = await _packageManagers.GetSupportedManagerAsync(
                PackageFilePath ?? throw new Exception("Package file path is null"),
                distroName,
                arg.Distro.Origin
            );

            var isInstalled = await packageManager.IsPackageInstalledAsync(
                distroName,
                PackageMetaData.PackageName
            );

            if (!isInstalled)
            {
                PackageInstallationStatus =
                    IPlatformDependentPackageManager.PackageInstallationStatus.NotInstalled;
                InstalledPackageVersion = String.Empty;
            }
            else
            {
                var installedPackageInfo = await packageManager.GetInstalledPackageInfoAsync(
                    distroName,
                    PackageMetaData.PackageName
                );

                var installationStatus = packageManager.CompareVersions(
                    installedPackageInfo.VersionCode,
                    PackageMetaData.VersionCode
                );

                PackageInstallationStatus = installationStatus;
                InstalledPackageVersion = installedPackageInfo.VersionCode;
            }
        }
        else
        {
            PackageInstallationStatus =
                IPlatformDependentPackageManager.PackageInstallationStatus.NotInstalled;
            InstalledPackageVersion = null;
        }

        return arg;
    }

    public string Id { get; } = nameof(PackageActionsViewModel);

    IPlatformDependentPackageManager.PackageMetaData _packageMetaData;

    public IPlatformDependentPackageManager.PackageMetaData PackageMetaData
    {
        get { return _packageMetaData; }
        set { this.RaiseAndSetIfChanged(ref _packageMetaData, value); }
    }

    WslDistributionModelView? _selectedWslDistribution;

    public WslDistributionModelView? SelectedWslDistribution
    {
        get { return _selectedWslDistribution; }
        set { this.RaiseAndSetIfChanged(ref _selectedWslDistribution, value); }
    }

    IPlatformDependentPackageManager.PackageInstallationStatus? _packageInstallationStatus;

    public IPlatformDependentPackageManager.PackageInstallationStatus? PackageInstallationStatus
    {
        get { return _packageInstallationStatus; }
        set { this.RaiseAndSetIfChanged(ref _packageInstallationStatus, value); }
    }

    string? _installedPackageVersion;

    public string? InstalledPackageVersion
    {
        get { return _installedPackageVersion; }
        set { this.RaiseAndSetIfChanged(ref _installedPackageVersion, value); }
    }

    private async Task ProcessPackageAsync()
    {
        InProgress = true;

        await Task.Delay(10);

        var distros = await Task.WhenAll(
            _distributionProviders.Select((dp) => dp.GetAllInstalledDistributionsAsync())
        );

        var supportedDistros = await GetSupportDistributionsAsync(distros.SelectMany(x => x));

        _distroSourceList.Edit(
            (list) =>
            {
                list.Clear();
                list.AddRange(supportedDistros.Select((d) => new WslDistributionModelView(d)));
            }
        );

        await _threadHelpers.UiThread;

        if (SelectedWslDistribution == null && _distroSourceList.Count > 0)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(0));

            SelectedWslDistribution = _distroSourceList.Items.First();
        }

        if (PackageMetaData.IconData?.Length > 0)
        {
            PackageIconStream = new MemoryStream(PackageMetaData.IconData);
        }
        else if (PackageMetaData.IconName != null)
        {
            PackageIconStream = await _iconThemeManager.ActiveIconTheme.GetSvgIconByNameAsync(
                PackageMetaData.IconName
            );
        }
        else
        {
            PackageIconStream = null;
        }

        InProgress = false;
    }

    private Task<IList<Distribution>> GetSupportDistributionsAsync(
        IEnumerable<Distribution> distros
    )
    {
        return distros
            .Where((d) => d.IsRunning)
            .WhereAsync(
                async (d) =>
                {
                    foreach (var packageManager in _packageManagers)
                    {
                        if (
                            await packageManager.IsSupportedByDistributionAsync(d.Name, d.Origin)
                            && (
                                await packageManager.IsPackageSupportedAsync(
                                    PackageFilePath
                                        ?? throw new Exception("Package file path is null")
                                )
                            ).isSupported
                        )
                        {
                            return true;
                        }
                    }

                    return false;
                }
            );
    }

    public IObservable<Unit> WhenNavigatedTo(INavigationParameter parameter)
    {
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> WhenNavigatedFrom(INavigationParameter parameter)
    {
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> WhenNavigatingTo(INavigationParameter parameter)
    {
        var navParms = parameter.FromNavigationParameter<NavigationParameter>();

        PackageMetaData = navParms.PackageMetaData;
        PackageFilePath = navParms.PackageFilePath;

        return ObservableAsync.From(ProcessPackageAsync, RxApp.MainThreadScheduler);
    }

    public ReactiveCommand<Unit, Unit> Close { get; }

    public ReadOnlyObservableCollection<WslDistributionModelView> DistroList => _distroList;

    public ReactiveCommand<Unit, Unit> ReInstallCommand { get; }

    public ReactiveCommand<Unit, Unit> InstallCommand { get; }

    public ReactiveCommand<Unit, Unit> UninstallCommand { get; }

    public ReactiveCommand<Unit, Unit> UpgradeCommand { get; }

    public ReactiveCommand<Unit, Unit> DowngradeCommand { get; }

    public ReactiveCommand<Unit, Unit> PrimaryPackageCommand { get; }
}
