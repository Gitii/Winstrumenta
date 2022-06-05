using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData;
using PackageInstaller.Core.Exceptions;
using PackageInstaller.Core.Helpers;
using PackageInstaller.Core.Services;
using ReactiveUI;
using Sextant;

namespace PackageInstaller.Core.ModelViews;

public class ActionExecutionViewModel : ReactiveObject, IViewModel, INavigable
{
    private DistributionModelView _distribution = null!;
    private readonly IEnumerable<IPlatformDependentPackageManager> _packageManagers;
    private PackageAction _selectedAction = PackageAction.Launch;
    FileSystemPath? _packageFilePath;
    private readonly IParameterViewStackService _viewStackService;
    private IPlatformDependentPackageManager.PackageMetaData _packageMetaData;
    private readonly Interaction<PopupInput, Unit> _showPopupInteraction;
    private Exception? _error;
    private readonly SourceList<OperationProgressModelView> _operationSourceList;
    private ReadOnlyObservableCollection<OperationProgressModelView> _operationList;
    private IThreadHelpers _threadHelpers;

    public ICommand RetryActionCommand { get; }

    public ICommand ShowErrorActions { get; }

    public ICommand CloseCommand { get; }

    string _packageLabel = String.Empty;

    public string PackageLabel
    {
        get { return _packageLabel; }
        set { this.RaiseAndSetIfChanged(ref _packageLabel, value); }
    }

    bool _actionFailed;

    public bool ActionFailed
    {
        get { return _actionFailed; }
        set { _threadHelpers.Schedule(() => this.RaiseAndSetIfChanged(ref _actionFailed, value)); }
    }

    public ActionExecutionViewModel(
        IParameterViewStackService viewStackService,
        IEnumerable<IPlatformDependentPackageManager> packageManagers,
        IThreadHelpers threadHelpers
    )
    {
        _viewStackService = viewStackService;
        _packageManagers = packageManagers;
        _threadHelpers = threadHelpers;

        _operationSourceList = new SourceList<OperationProgressModelView>();
        _operationSourceList
            .Connect()
            .AutoRefresh()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _operationList)
            .DisposeMany()
            .Subscribe();

        RetryActionCommand = ReactiveCommand.CreateFromTask(ExecuteAsync);
        CloseCommand = ReactiveCommand.Create(() => Environment.Exit(0));
        ShowErrorActions = ReactiveCommand.Create(ShowErrorInPopup);

        _showPopupInteraction = new Interaction<PopupInput, Unit>();
    }

    private void ShowErrorInPopup()
    {
        var content = GetErrorMessageFromException(_error);

        ShowPopupInteraction
            .Handle(new PopupInput() { Title = "Error details", Content = content, })
            .Subscribe();
    }

    private string GetErrorMessageFromException(Exception? error)
    {
        return error switch
        {
            null => string.Empty,
            DetailedException ex => Join(ex.Details, ex.Message, ex.StackTrace),
            var genericException => Join(genericException.Message, genericException.StackTrace),
        };

        string Join(params string?[] parts)
        {
            return String.Join(Environment.NewLine, parts.Where((p) => !string.IsNullOrEmpty(p)));
        }
    }

    public readonly struct NavigationParameter
    {
        public DistributionModelView Distribution { get; init; }
        public PackageAction SelectedAction { get; init; }
        public FileSystemPath PackageFilePath { get; init; }
        public IPlatformDependentPackageManager.PackageMetaData PackageMetaData { get; init; }
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

        _distribution = navParms.Distribution;
        _selectedAction = navParms.SelectedAction;
        _packageFilePath = navParms.PackageFilePath;
        _packageMetaData = navParms.PackageMetaData;

        PackageLabel = navParms.PackageMetaData.PackageLabel;

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
        RxApp.MainThreadScheduler.Schedule(
            async () =>
            {
                await this.ExecuteAsync().ConfigureAwait(false);
            }
        );
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

        return Observable.Return(Unit.Default);
    }

    private async Task ExecuteAsync()
    {
        _operationSourceList.Clear();

        var progressController = new ProgressController(
            _operationSourceList,
            RxApp.MainThreadScheduler
        );

        try
        {
            ActionFailed = false;

            var distroMv = _distribution;

            var (success, log) = await ExecuteActionAsync(progressController).ConfigureAwait(false);

            if (!success)
            {
                throw new DetailedException(GetActionFailureErrorMessage(), log);
            }

            var navParms = new ResultViewModel.NavigationParameter()
            {
                Title = GetActionSuccessTitle(),
                Description = GetActionSuccessDescription(distroMv),
                Details = log
            };

            _viewStackService
                .PushPage<ResultViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
        catch (Exception e)
        {
            if (!(_operationSourceList.Items.LastOrDefault()?.HasFailed ?? true))
            {
                progressController.Fail(e.Message);
            }

            ActionFailed = true;
            _error = e;
        }
    }

    private async Task<(bool success, string log)> ExecuteActionAsync(
        IProgressController progressController
    )
    {
        var distroMv = _distribution;
        var distroName = distroMv.Distro.Id;
        var packageFilePath = _packageFilePath ?? throw new Exception("Package file path is null");

        var packageManager = await _packageManagers
            .GetSupportedManagerAsync(packageFilePath, distroName, distroMv.Distro.Origin)
            .ConfigureAwait(false);

        switch (_selectedAction)
        {
            case PackageAction.Launch:
                await packageManager
                    .LaunchAsync(distroName, _packageMetaData.PackageName, progressController)
                    .ConfigureAwait(false);
                return (true, String.Empty);
            case PackageAction.Install:
                return await packageManager
                    .InstallAsync(distroName, packageFilePath, progressController)
                    .ConfigureAwait(false);
            case PackageAction.Uninstall:
                return await packageManager
                    .UninstallAsync(distroName, _packageMetaData.PackageName, progressController)
                    .ConfigureAwait(false);
            case PackageAction.Upgrade:
                return await packageManager
                    .UpgradeAsync(distroName, packageFilePath, progressController)
                    .ConfigureAwait(false);
            case PackageAction.Downgrade:
                return await packageManager
                    .DowngradeAsync(distroName, packageFilePath, progressController)
                    .ConfigureAwait(false);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private string GetActionFailureErrorMessage()
    {
        return _selectedAction switch
        {
            PackageAction.Launch => "Failed to launch package",
            PackageAction.Install => "Failed to install package",
            PackageAction.Uninstall => "Failed to uninstall package",
            PackageAction.Upgrade => "Failed to upgrade package",
            PackageAction.Downgrade => "Failed to downgrade package",
            _ => throw new ArgumentOutOfRangeException(nameof(_selectedAction))
        };
    }

    private string GetActionSuccessDescription(DistributionModelView distroMv)
    {
        return _selectedAction switch
        {
            PackageAction.Launch
              => $"The package '{_packageMetaData.PackageLabel}' ({_packageMetaData.VersionLabel}) has been launched in '{distroMv.Name}'.",
            PackageAction.Install
              => $"The package '{_packageMetaData.PackageLabel}' ({_packageMetaData.VersionLabel}) has been installed in '{distroMv.Name}'.",
            PackageAction.Uninstall
              => $"The package '{_packageMetaData.PackageLabel}' ({_packageMetaData.VersionLabel}) has been uninstalled in '{distroMv.Name}'.",
            PackageAction.Upgrade
              => $"The package '{_packageMetaData.PackageLabel}' ({_packageMetaData.VersionLabel}) has been upgraded in '{distroMv.Name}'.",
            PackageAction.Downgrade
              => $"The package '{_packageMetaData.PackageLabel}' ({_packageMetaData.VersionLabel}) has been downgraded in '{distroMv.Name}'.",
            _ => throw new ArgumentOutOfRangeException(nameof(_selectedAction))
        };
    }

    private string GetActionSuccessTitle()
    {
        return _selectedAction switch
        {
            PackageAction.Launch => "Package has been launched",
            PackageAction.Install => "Package has been installed successfully",
            PackageAction.Uninstall => "Package has been uninstalled successfully",
            PackageAction.Upgrade => "Package has been upgraded successfully",
            PackageAction.Downgrade => "Package has been downgraded successfully",
            _ => throw new ArgumentOutOfRangeException(nameof(_selectedAction))
        };
    }

    public string Id { get; } = nameof(ActionExecutionViewModel);

    public ReadOnlyObservableCollection<OperationProgressModelView> OperationList => _operationList;

    public Interaction<PopupInput, Unit> ShowPopupInteraction => _showPopupInteraction;
}
