using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using DynamicData;
using Microsoft.Extensions.Hosting;
using PackageInstaller.Core.Services;
using ReactiveUI;
using Sextant;
using Shared.Misc;
using Shared.Services;

namespace PackageInstaller.Core.ModelViews;

public class PackageActionsViewModel : ReactiveObject, IViewModel, INavigable
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IList<IDistributionProvider> _distributionProviders;
    private readonly ReadOnlyObservableCollection<DistributionModelView> _distroList;
    private readonly SourceList<DistributionModelView> _distroSourceList;
    private readonly ActionModelView _doNothingActionMv;
    private readonly ActionModelView _downgradeActionMv;

    private readonly ActionModelView _installActionMv;
    private readonly ActionModelView _launchActionMv;

    readonly ObservableAsPropertyHelper<DistributionList.AlertPriority> _notificationIconType;
    private readonly ActionModelView _reinstallActionMv;
    private readonly ActionModelView _uninstallActionMv;
    private readonly ActionModelView _upgradeActionMv;
    private readonly IParameterViewStackService _viewStackService;
    private IIconThemeManager _iconThemeManager;
    bool _inProgress;

    string? _installedPackageVersion;
    FileSystemPath? _packageFilePath;
    Stream? _packageIconStream;

    IPlatformDependentPackageManager.PackageInstallationStatus? _packageInstallationStatus;
    private IEnumerable<IPlatformDependentPackageManager> _packageManagers;

    IPlatformDependentPackageManager.PackageMetaData _packageMetaData;
    private IPath _path;
    private ObservableAsPropertyHelper<ActionModelView?> _primaryAction;
    string _progressStatusMessage;
    private ObservableAsPropertyHelper<IImmutableList<ActionModelView>> _secondaryActions;

    DistributionModelView? _selectedWslDistribution;
    private IThreadHelpers _threadHelpers;
    private readonly IApplicationLifeCycle _lifeCycle;
    private readonly Interaction<string, bool> _actionConfirmationDialogInteraction;

#pragma warning disable MA0051 // Method is too long
    public PackageActionsViewModel(
#pragma warning restore MA0051 // Method is too long
        IHostApplicationLifetime applicationLifetime,
        IEnumerable<IDistributionProvider> distributionProviders,
        IParameterViewStackService viewStackService,
        IEnumerable<IPlatformDependentPackageManager> packageManagers,
        IPath path,
        IIconThemeManager iconThemeManager,
        IThreadHelpers threadHelpers,
        IApplicationLifeCycle lifeCycle
    )
    {
        _applicationLifetime = applicationLifetime;
        _distributionProviders = distributionProviders.ToList();
        _viewStackService = viewStackService;
        _packageManagers = packageManagers;
        _path = path;
        _iconThemeManager = iconThemeManager;
        _threadHelpers = threadHelpers;
        _lifeCycle = lifeCycle;

        _actionConfirmationDialogInteraction = new Interaction<string, bool>();

        _progressStatusMessage = String.Empty;
        ProgressStatusMessage = String.Empty;
        _notifications = ImmutableList<NotificationModelView>.Empty;

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
                _lifeCycle.Exit(0);
            }
        );

        GotoNotificationHub = ReactiveCommand.Create(
            () =>
            {
                var navParams = new NotificationHubModelView.NavigationParameter()
                {
                    Notifications = Notifications,
                };

                _viewStackService
                    .PushPage<NotificationHubModelView>(navParams.ToNavigationParameter())
                    .Subscribe();
            }
        );

        this.WhenAnyValue((vm) => vm.SelectedWslDistribution)
            .ObserveOn(RxApp.MainThreadScheduler)
            .SelectMany(OnSelectedDistributionChangedAsync)
            .Subscribe();

        _launchActionMv = new ActionModelView(
            BuildCommandFunction(PackageAction.Launch),
            "Launch",
            tooltip: "Launch the application"
        );
        _installActionMv = new ActionModelView(
            BuildCommandFunction(PackageAction.Install),
            "Install",
            tooltip: "Install this application in the selected distribution"
        );
        _uninstallActionMv = new ActionModelView(
            BuildCommandFunction(PackageAction.Uninstall),
            "Uninstall",
            tooltip: "Uninstall this application in the selected distribution"
        );
        _reinstallActionMv = new ActionModelView(
            BuildCommandFunction(PackageAction.Install),
            "Reinstall",
            tooltip: "Reinstall this application in the selected distribution"
        );
        _upgradeActionMv = new ActionModelView(
            BuildCommandFunction(PackageAction.Upgrade),
            "Upgrade",
            tooltip: "Upgrade this application in the selected distribution"
        );
        _downgradeActionMv = new ActionModelView(
            BuildCommandFunction(PackageAction.Downgrade),
            "Downgrade",
            "This action may not do any dependency checking on downgrades "
                + "and therefore will not warn you if the downgrade breaks the dependency "
                + "of some other package. This can have serious side effects, downgrading "
                + "essential system components can even make your whole system unusable. "
                + "Use with care."
        );
        _doNothingActionMv = new ActionModelView(
            (_) => _applicationLifetime.StopApplication(),
            "Do nothing and close"
        );

        _primaryAction = this.WhenAnyValue((mv) => mv.PackageInstallationStatus)
            .Select(ChoosePrimaryAction)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, (mv) => mv.PrimaryAction);

        _secondaryActions = this.WhenAnyValue((mv) => mv.PackageInstallationStatus)
            .Select(ChooseSecondaryActions)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, (mv) => mv.SecondaryActions);

        _notificationIconType = this.WhenAnyValue((mv) => mv.Notifications)
            .Select(
                (x) =>
                    x.MaxBy((n) => n.Priority)?.Priority
                    ?? DistributionList.AlertPriority.Information
            )
            .ToProperty(this, (mv) => mv.NotificationIconType);
    }

    ImmutableList<NotificationModelView> _notifications;

    public ImmutableList<NotificationModelView> Notifications
    {
        get { return _notifications; }
        set { this.RaiseAndSetIfChanged(ref _notifications, value); }
    }

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

    public IPlatformDependentPackageManager.PackageMetaData PackageMetaData
    {
        get { return _packageMetaData; }
        set { this.RaiseAndSetIfChanged(ref _packageMetaData, value); }
    }

    public DistributionModelView? SelectedWslDistribution
    {
        get { return _selectedWslDistribution; }
        set { this.RaiseAndSetIfChanged(ref _selectedWslDistribution, value); }
    }

    public IPlatformDependentPackageManager.PackageInstallationStatus? PackageInstallationStatus
    {
        get { return _packageInstallationStatus; }
        set { this.RaiseAndSetIfChanged(ref _packageInstallationStatus, value); }
    }

    public string? InstalledPackageVersion
    {
        get { return _installedPackageVersion; }
        set { this.RaiseAndSetIfChanged(ref _installedPackageVersion, value); }
    }

    public ReactiveCommand<Unit, Unit> GotoNotificationHub { get; }

    public ReactiveCommand<Unit, Unit> Close { get; }

    public ReadOnlyObservableCollection<DistributionModelView> DistroList => _distroList;

    public ActionModelView? PrimaryAction => _primaryAction.Value;

    public IImmutableList<ActionModelView> SecondaryActions => _secondaryActions.Value;

    public DistributionList.AlertPriority NotificationIconType => _notificationIconType.Value;

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

    public string Id { get; } = nameof(PackageActionsViewModel);

    public Interaction<string, bool> ActionConfirmationDialogInteraction =>
        _actionConfirmationDialogInteraction;

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

    private Func<ActionModelView, Task> BuildCommandFunction(PackageAction action)
    {
        return async (ActionModelView avm) =>
        {
            if (avm.WarningText != null)
            {
                var confirmedByUser = await _actionConfirmationDialogInteraction
                    .Handle(avm.WarningText)
                    .ToTask()
                    .ConfigureAwait(true);
                if (!confirmedByUser)
                {
                    return;
                }
            }

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

            var packageManager = await _packageManagers
                .GetSupportedManagerAsync(
                    PackageFilePath ?? throw new Exception("Package file path is null"),
                    distroName,
                    arg.Distro.Origin
                )
                .ConfigureAwait(true);

            var isInstalled = await packageManager
                .IsPackageInstalledAsync(distroName, PackageMetaData.PackageName)
                .ConfigureAwait(true);

            if (!isInstalled)
            {
                PackageInstallationStatus = IPlatformDependentPackageManager
                    .PackageInstallationStatus
                    .NotInstalled;
                InstalledPackageVersion = String.Empty;
            }
            else
            {
                var installedPackageInfo = await packageManager
                    .GetInstalledPackageInfoAsync(distroName, PackageMetaData.PackageName)
                    .ConfigureAwait(true);

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

    private async Task ProcessPackageAsync()
    {
        InProgress = true;

        await Task.Delay(10).ConfigureAwait(true);

        var ext = Path.GetExtension(PackageFilePath?.WindowsPath ?? "").TrimStart('.');

        var distributionLists = await Task.WhenAll(
                _distributionProviders.Select((dp) => dp.GetAllInstalledDistributionsAsync(ext))
            )
            .ConfigureAwait(true);

        var installedDistributions = await GetSupportDistributionsAsync(
                distributionLists.SelectMany(x => x.InstalledDistributions)
            )
            .ConfigureAwait(true);

        _distroSourceList.Edit(
            (list) =>
            {
                list.Clear();
                list.AddRange(installedDistributions.Select((d) => new DistributionModelView(d)));
            }
        );

        await _threadHelpers.UiThread;

        Notifications = distributionLists
            .SelectMany((x) => x.Alerts)
            .OrderBy((x) => x.Priority)
            .ThenBy((x) => x.Title)
            .Select((x) => new NotificationModelView(x))
            .ToImmutableList();

        if (SelectedWslDistribution == null && _distroSourceList.Count > 0)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(true);

            SelectedWslDistribution = _distroSourceList.Items.First();
        }

        if (PackageMetaData.IconData?.Length > 0)
        {
            PackageIconStream = new MemoryStream(PackageMetaData.IconData);
        }
        else if (PackageMetaData.IconName != null)
        {
            PackageIconStream = await _iconThemeManager.ActiveIconTheme
                .GetSvgIconByNameAsync(PackageMetaData.IconName)
                .ConfigureAwait(true);
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
                            await packageManager
                                .IsSupportedByDistributionAsync(d.Id, d.Origin)
                                .ConfigureAwait(true)
                            && (
                                await packageManager
                                    .IsPackageSupportedAsync(
                                        PackageFilePath
                                            ?? throw new Exception("Package file path is null")
                                    )
                                    .ConfigureAwait(true)
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

    public readonly struct NavigationParameter
    {
        public readonly IPlatformDependentPackageManager.PackageMetaData PackageMetaData { get; init; }

        public readonly FileSystemPath PackageFilePath { get; init; }
    }
}
