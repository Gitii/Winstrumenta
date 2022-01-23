using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using JetBrains.Profiler.Api;
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
public class PreparationViewModel : ReactiveObject, IViewModel, INavigable
{
    private IEnumerable<IPlatformDependentPackageManager> _packagesManagers;
    private IPath _path;
    private IIconThemeManager _iconThemeManager;

    public readonly struct NavigationParameter
    {
        public string[] Arguments { get; init; }
    }

    private readonly IParameterViewStackService _viewStackService;
    public string Id { get; } = nameof(PreparationViewModel);

    public PreparationViewModel(
        IParameterViewStackService viewStackService,
        IEnumerable<IPlatformDependentPackageManager> packagesManagers,
        IPath path,
        IIconThemeManager iconThemeManager
    )
    {
        _viewStackService = viewStackService;
        _packagesManagers = packagesManagers;
        _path = path;
        _iconThemeManager = iconThemeManager;
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

        return ProcessAsync(navParms.Arguments).ToObservable(RxApp.MainThreadScheduler);
    }

    public async Task ProcessAsync(string[] arguments)
    {
        await Task.Delay(10).ConfigureAwait(false);

        try
        {
            MemoryProfiler.CollectAllocations(true);
            MeasureProfiler.StartCollectingData();
            await _iconThemeManager.LoadThemesAsync().ConfigureAwait(false);
            MeasureProfiler.SaveData("Loaded themes");
            MemoryProfiler.GetSnapshot("Loaded themes");

            var packageFilePath = ParseArguments(arguments);

            var packageManager = await _packagesManagers
                .GetSupportedManagerAsync(packageFilePath)
                .ConfigureAwait(false);

            MemoryProfiler.CollectAllocations(true);
            MeasureProfiler.StartCollectingData();
            var data = await packageManager
                .ExtractPackageMetaDataAsync(packageFilePath)
                .ConfigureAwait(false);
            MeasureProfiler.SaveData("Extracted archive metadata");
            MemoryProfiler.GetSnapshot("Extracted archive metadata");

            var navParms = new PackageActionsViewModel.NavigationParameter()
            {
                PackageFilePath = packageFilePath,
                PackageMetaData = data
            };

            _viewStackService
                .PushPage<PackageActionsViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
        catch (Exception e)
        {
            var navParms = new ErrorViewModel.NavigationParameter() { Exception = e };

            _viewStackService
                .PushPage<ErrorViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
    }

    private FileSystemPath ParseArguments(string[] arguments)
    {
        if (arguments.Length == 0)
        {
            throw new Exception(
                "No arguments passed in. The first argument must be the file path to the debian package."
            );
        }
        else if (arguments.Length > 1)
        {
            throw new Exception(
                "Too many arguments passed in. The first argument must be the file path to the debian package."
            );
        }

        var filePath = arguments[0];

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The file doesn't exist", filePath);
        }

        return _path.ToFileSystemPath(filePath);
    }
}
