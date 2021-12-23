using System.Collections.ObjectModel;
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

    public readonly struct NavigationParameter
    {
        public readonly PackageMetaData PackageMetaData { get; init; }

        public readonly string PackageFilePath { get; init; }
    }

    public PackageActionsViewModel(
        IHostApplicationLifetime applicationLifetime,
        IWsl wsl,
        IPackageManager packageManager
    )
    {
        _applicationLifetime = applicationLifetime;
        _wsl = wsl;
        _packageManager = packageManager;

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

        InstallCommand = ReactiveCommand.CreateFromTask(Install);
    }

    private Task Install() { }

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

    IPackageManager.PackageInstallationStatus? __packageInstallationStatus;
    public IPackageManager.PackageInstallationStatus? _packageInstallationStatus
    {
        get { return __packageInstallationStatus; }
        set { this.RaiseAndSetIfChanged(ref __packageInstallationStatus, value); }
    }

    string? _installedPackageVersion;
    public string? InstalledPackageVersion
    {
        get { return _installedPackageVersion; }
        set { this.RaiseAndSetIfChanged(ref _installedPackageVersion, value); }
    }

    private async Task ProcessPackage()
    {
        //_wsl.AssertWslIsReady();
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
            SelectedWslDistribution = _distroSourceList.Items.First();
        }
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

        return Observable.FromAsync(ProcessPackage);
    }

    public ReactiveCommand<Unit, Unit> Close { get; }

    public ReadOnlyObservableCollection<WslDistributionModelView> DistroList => _distroList;

    public ReactiveCommand<Unit, Unit> InstallCommand { get; }

    public ReactiveCommand<Unit, Unit> UninstallCommand { get; }

    public ReactiveCommand<Unit, Unit> UpgradeCommand { get; }

    public ReactiveCommand<Unit, Unit> DowngradeCommand { get; }
}
