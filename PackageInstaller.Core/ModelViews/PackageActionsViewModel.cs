using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using Microsoft.Extensions.Hosting;
using PackageInstaller.Core.Helpers;
using PackageInstaller.Core.Services;
using ReactiveUI;
using Sextant;

namespace PackageInstaller.Core.ModelViews;

public class PackageActionsViewModel : ReactiveObject, IViewModel, INavigable
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IWsl _wsl;
    private readonly SourceList<WslDistributionModelView> _distroSourceList;
    private readonly ReadOnlyObservableCollection<WslDistributionModelView> _distroList;
    private IPackageManager _packageManager;
    bool _inProgress;
    string _progressStatusMessage;
    string _packageFilePath;
    private readonly IParameterViewStackService _viewStackService;

    public string PackageFilePath
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
        public readonly PackageMetaData PackageMetaData { get; init; }

        public readonly string PackageFilePath { get; init; }
    }

    public PackageActionsViewModel(
        IHostApplicationLifetime applicationLifetime,
        IWsl wsl,
        IPackageManager packageManager,
        IParameterViewStackService viewStackService
    )
    {
        _applicationLifetime = applicationLifetime;
        _wsl = wsl;
        _packageManager = packageManager;
        _viewStackService = viewStackService;

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
                Environment.Exit(1);
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
            case IPackageManager.PackageInstallationStatus.NotInstalled:
                return Install();
            case IPackageManager.PackageInstallationStatus.InstalledSameVersion:
                return ReInstall();
            case IPackageManager.PackageInstallationStatus.InstalledOlderVersion:
                return Upgrade();
            case IPackageManager.PackageInstallationStatus.InstalledNewerVersion:
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

            var log = await _packageManager.InstallPackage(
                SelectedWslDistribution!.Distro,
                PackageFilePath
            );

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

            var log = await _packageManager.InstallPackage(
                SelectedWslDistribution!.Distro,
                PackageFilePath
            );

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

            var log = await _packageManager.InstallPackage(
                SelectedWslDistribution!.Distro,
                PackageFilePath
            );

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
            var status = await _packageManager.CheckInstallationStatus(
                arg.Distro,
                PackageMetaData.Package,
                PackageMetaData.Version
            );

            PackageInstallationStatus = status.Status;
            InstalledPackageVersion = status.InstalledPackageVersion;
        }
        else
        {
            PackageInstallationStatus = IPackageManager.PackageInstallationStatus.NotInstalled;
            InstalledPackageVersion = null;
        }

        return arg;
    }

    public string Id { get; } = nameof(PackageActionsViewModel);

    PackageMetaData _packageMetaData;

    public PackageMetaData PackageMetaData
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

    IPackageManager.PackageInstallationStatus? _packageInstallationStatus;

    public IPackageManager.PackageInstallationStatus? PackageInstallationStatus
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

        _distroSourceList.Edit(
            (list) =>
            {
                list.Clear();
                list.AddRange(
                    distros
                        .Where(
                            (d) =>
                                d.IsRunning
                                && d.SupportedPackageTypes.HasFlag(PackageMetaData.PackageType)
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