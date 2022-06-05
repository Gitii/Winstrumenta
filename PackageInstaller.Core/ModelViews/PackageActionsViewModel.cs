using System.Collections.Immutable;
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

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "MA0004:Use Task.ConfigureAwait(false)",
    Justification = "ModelView should care about thread context."
)]
public class PackageActionsViewModel : ReactiveObject, IViewModel, INavigable
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IList<IDistributionProvider> _distributionProviders;
    private readonly SourceList<DistributionModelView> _distroSourceList;
    private readonly ReadOnlyObservableCollection<DistributionModelView> _distroList;
    private IEnumerable<IPlatformDependentPackageManager> _packageManagers;
    bool _inProgress;
    string _progressStatusMessage;
    FileSystemPath? _packageFilePath;
    private readonly IParameterViewStackService _viewStackService;
    private IPath _path;
    Stream? _packageIconStream;
    private IThreadHelpers _threadHelpers;
    private IIconThemeManager _iconThemeManager;
    private ObservableAsPropertyHelper<ActionModelView?> _primaryAction;
    private ObservableAsPropertyHelper<IImmutableList<ActionModelView>> _secondaryActions;

    private readonly ActionModelView _installActionMv;
    private readonly ActionModelView _uninstallActionMv;
    private readonly ActionModelView _reinstallActionMv;
    private readonly ActionModelView _upgradeActionMv;
    private readonly ActionModelView _downgradeActionMv;
    private readonly ActionModelView _doNothingActionMv;
    private readonly ActionModelView _launchActionMv;

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

#pragma warning disable MA0051 // Method is too long
    public PackageActionsViewModel(
#pragma warning restore MA0051 // Method is too long
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

        _distroSourceList = new SourceList<DistributionModelView>();
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

        _launchActionMv = new ActionModelView(BuildCommandFunction(PackageAction.Launch), "Launch");
        _installActionMv = new ActionModelView(
            BuildCommandFunction(PackageAction.Install),
            "Install"
        );
        _uninstallActionMv = new ActionModelView(
            BuildCommandFunction(PackageAction.Uninstall),
            "Uninstall"
        );
        _reinstallActionMv = new ActionModelView(
            BuildCommandFunction(PackageAction.Install),
            "Reinstall"
        );
        _upgradeActionMv = new ActionModelView(
            BuildCommandFunction(PackageAction.Upgrade),
            "Upgrade"
        );
        _downgradeActionMv = new ActionModelView(
            BuildCommandFunction(PackageAction.Downgrade),
            "Downgrade",
            "This action may not do any dependency checking on downgrades "
                + "and therefore will not warn you if the downgrade breaks the dependency "
                + "of some other package.This can have serious side effects, downgrading "
                + "essential system components can even make your whole system unusable. "
                + "Use with care."
        );
        _doNothingActionMv = new ActionModelView(
            () => _applicationLifetime.StopApplication(),
            "Do nothing and exit"
        );

        _primaryAction = this.WhenAnyValue((mv) => mv.PackageInstallationStatus)
            .Select(ChoosePrimaryAction)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, (mv) => mv.PrimaryAction);

        _secondaryActions = this.WhenAnyValue((mv) => mv.PackageInstallationStatus)
            .Select(ChooseSecondaryActions)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, (mv) => mv.SecondaryActions);
    }

    private IImmutableList<ActionModelView> ChooseSecondaryActions(
        IPlatformDependentPackageManager.PackageInstallationStatus? arg
    )
    {
        switch (arg)
        {
            case IPlatformDependentPackageManager.PackageInstallationStatus.NotInstalled:
                return ImmutableList.Create<ActionModelView>();
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledSameVersion:
                return ImmutableList.Create<ActionModelView>(
                    _reinstallActionMv,
                    _uninstallActionMv
                );
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledOlderVersion:
                return ImmutableList.Create<ActionModelView>(_uninstallActionMv, _launchActionMv);
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledNewerVersion:
                return ImmutableList.Create<ActionModelView>(
                    _downgradeActionMv,
                    _uninstallActionMv,
                    _launchActionMv
                );
            case null:
                return ImmutableList.Create<ActionModelView>();
            default:
                throw new ArgumentOutOfRangeException(nameof(arg), arg, null);
        }
    }

    private ActionModelView? ChoosePrimaryAction(
        IPlatformDependentPackageManager.PackageInstallationStatus? arg
    )
    {
        switch (arg)
        {
            case IPlatformDependentPackageManager.PackageInstallationStatus.NotInstalled:
                return _installActionMv;
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledSameVersion:
                return _launchActionMv;
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledOlderVersion:
                return _upgradeActionMv;
            case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledNewerVersion:
                return _doNothingActionMv;
            case null:
                return null;
            default:
                throw new ArgumentOutOfRangeException(nameof(arg), arg, null);
        }
    }

    private Action BuildCommandFunction(PackageAction action)
    {
        return () =>
        {
            var navParms = new ActionExecutionViewModel.NavigationParameter()
            {
                Distribution = SelectedWslDistribution!,
                PackageFilePath = PackageFilePath!,
                PackageMetaData = PackageMetaData,
                SelectedAction = action,
            };

            _viewStackService
                .PushPage<ActionExecutionViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        };
    }

    private async Task<DistributionModelView?> OnSelectedDistributionChangedAsync(
        DistributionModelView? arg
    )
    {
        if (arg != null)
        {
            var distroName = arg.Distro.Id;

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
                PackageInstallationStatus = IPlatformDependentPackageManager
                    .PackageInstallationStatus
                    .NotInstalled;
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
            PackageInstallationStatus = IPlatformDependentPackageManager
                .PackageInstallationStatus
                .NotInstalled;
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

    DistributionModelView? _selectedWslDistribution;

    public DistributionModelView? SelectedWslDistribution
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
                list.AddRange(supportedDistros.Select((d) => new DistributionModelView(d)));
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
                            await packageManager.IsSupportedByDistributionAsync(d.Id, d.Origin)
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

    public ReadOnlyObservableCollection<DistributionModelView> DistroList => _distroList;

    public ActionModelView? PrimaryAction => _primaryAction.Value;

    public IImmutableList<ActionModelView> SecondaryActions => _secondaryActions.Value;
}
