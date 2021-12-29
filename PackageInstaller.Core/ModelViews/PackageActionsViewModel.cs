using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using Microsoft.Extensions.Hosting;
using PackageInstaller.Core.Exceptions;
using PackageInstaller.Core.Helpers;
using PackageInstaller.Core.Services;
using ReactiveUI;
using Sextant;
using Z.Linq;

namespace PackageInstaller.Core.ModelViews;

public class PackageActionsViewModel : ReactiveObject, IViewModel, INavigable
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IWsl _wsl;
    private readonly SourceList<WslDistributionModelView> _distroSourceList;
    private readonly ReadOnlyObservableCollection<WslDistributionModelView> _distroList;
    private IEnumerable<IPlatformDependentPackageManager> _packageManagers;
    bool _inProgress;
    string _progressStatusMessage;
    FileSystemPath _packageFilePath;
    private readonly IParameterViewStackService _viewStackService;
    private IPath _path;

    public FileSystemPath PackageFilePath
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
        IWsl wsl,
        IParameterViewStackService viewStackService,
        IEnumerable<IPlatformDependentPackageManager> packageManagers,
        IPath path
    )
    {
        _applicationLifetime = applicationLifetime;
        _wsl = wsl;
        _viewStackService = viewStackService;
        _packageManagers = packageManagers;
        _path = path;

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
            .SelectMany(OnSelectedDistributonChanged)
            .Subscribe();

        PrimaryPackageCommand = ReactiveCommand.CreateFromTask(ExecutePrimaryCommand);

        InstallCommand = ReactiveCommand.CreateFromTask(Install);
        ReInstallCommand = ReactiveCommand.CreateFromTask(ReInstall);
        UpgradeCommand = ReactiveCommand.CreateFromTask(Upgrade);
        DowngradeCommand = ReactiveCommand.CreateFromTask(Downgrade);
        UninstallCommand = ReactiveCommand.CreateFromTask(Uninstall);
    }

    private Task Uninstall()
    {
        return Task.CompletedTask;
    }

    private Task ExecutePrimaryCommand()
    {
        switch (PackageInstallationStatus)
        {
            case IPlatformDependentPackageManager.PackageInstallationStatus.NotInstalled:
                return Install();
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledSameVersion:
                return ReInstall();
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledOlderVersion:
                return Upgrade();
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledNewerVersion:
                return Downgrade();
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(PackageInstallationStatus),
                    PackageInstallationStatus?.ToString()
                );
        }
    }

    private async Task Downgrade()
    {
        try
        {
            InProgress = true;
            ProgressStatusMessage = String.Empty;

            ProgressStatusMessage = "Downgrading package...";

            var distroName = SelectedWslDistribution!.Name;

            var packageManager = await _packageManagers.GetSupportedManager(
                PackageFilePath,
                distroName
            );

            var (success, log) = await packageManager.Downgrade(distroName, PackageFilePath);

            if (!success)
            {
                throw new DetailedException("Failed to install package", log);
            }

            ProgressStatusMessage = "Package downgraded!";

            var navParms = new ResultViewModel.NavigationParameter()
            {
                Title = "Package has been downgraded successfully",
                Description =
                    $"The package '{PackageMetaData.Package}' has been downgraded to '{PackageMetaData.Version}' in '{SelectedWslDistribution.Name}'.",
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

    private async Task Upgrade()
    {
        try
        {
            InProgress = true;
            ProgressStatusMessage = String.Empty;

            ProgressStatusMessage = "Upgrading package...";

            var distroName = SelectedWslDistribution!.Name;

            var packageManager = await _packageManagers.GetSupportedManager(
                PackageFilePath,
                distroName
            );

            var (success, log) = await packageManager.Upgrade(distroName, PackageFilePath);

            if (!success)
            {
                throw new DetailedException("Failed to install package", log);
            }

            ProgressStatusMessage = "Package upgraded!";

            var navParms = new ResultViewModel.NavigationParameter()
            {
                Title = "Package has been upgraded successfully",
                Description =
                    $"The package '{PackageMetaData.Package}' has been upgraded to '{PackageMetaData.Version}' in '{SelectedWslDistribution.Name}'.",
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

    private Task ReInstall()
    {
        return Install();
    }

    private async Task Install()
    {
        try
        {
            InProgress = true;
            ProgressStatusMessage = String.Empty;

            ProgressStatusMessage = "Installing package...";

            var distroName = SelectedWslDistribution!.Name;

            var packageManager = await _packageManagers.GetSupportedManager(
                PackageFilePath,
                distroName
            );

            var (success, log) = await packageManager.Install(distroName, PackageFilePath);

            if (!success)
            {
                throw new DetailedException("Failed to install package", log);
            }

            ProgressStatusMessage = "Package installed!";

            var navParms = new ResultViewModel.NavigationParameter()
            {
                Title = "Package has been installed successfully",
                Description =
                    $"The package '{PackageMetaData.Package}' has been installed in '{SelectedWslDistribution.Name}'.",
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

    private async Task<WslDistributionModelView?> OnSelectedDistributonChanged(
        WslDistributionModelView? arg
    )
    {
        if (arg != null)
        {
            var distroName = SelectedWslDistribution!.Name;

            var packageManager = await _packageManagers.GetSupportedManager(
                PackageFilePath,
                distroName
            );

            var isInstalled = await packageManager.IsPackageInstalled(
                distroName,
                PackageMetaData.Package
            );

            if (!isInstalled)
            {
                PackageInstallationStatus =
                    IPlatformDependentPackageManager.PackageInstallationStatus.NotInstalled;
                InstalledPackageVersion = String.Empty;
            }
            else
            {
                var installedPackageInfo = await packageManager.GetInstalledPackageInfo(
                    distroName,
                    PackageMetaData.Package
                );

                var installationStatus = packageManager.CompareVersions(
                    installedPackageInfo.Version,
                    PackageMetaData.Version
                );

                PackageInstallationStatus = installationStatus;
                InstalledPackageVersion = installedPackageInfo.Version;
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

    WslDistributionModelView _selectedWslDistribution;

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

    private async Task ProcessPackage()
    {
        InProgress = true;

        var distros = await _wsl.GetAllInstalledDistributions();

        var supportedDistros = await distros.WhereAsync(
            async (d) =>
            {
                foreach (var packageManager in _packageManagers)
                {
                    if (await packageManager.IsSupportedByDistribution(d.Name))
                    {
                        return true;
                    }
                }

                return false;
            }
        );

        _distroSourceList.Edit(
            (list) =>
            {
                list.Clear();
                list.AddRange(
                    supportedDistros
                        .Where(
                            (d) =>
                                d.IsRunning
                                && d.Name is not ("docker-desktop-data" or "docker-desktop")
                        )
                        .Select((d) => new WslDistributionModelView(d))
                );
            }
        );

        if (SelectedWslDistribution == null && _distroSourceList.Count > 0)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            SelectedWslDistribution = _distroSourceList.Items.First();
        }

        InProgress = false;
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

        return ObservableAsync.FromAsync(ProcessPackage);
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