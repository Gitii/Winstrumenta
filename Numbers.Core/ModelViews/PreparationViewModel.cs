using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Numbers.Core.Services;
using ReactiveUI;
using Sextant;
using Shared.Misc;
using Shared.Services;

namespace Numbers.Core.ModelViews;

public class PreparationViewModel : ReactiveObject, IViewModel, INavigable
{
    private readonly IPath _path;
    private readonly IFile _file;
    private readonly IGlobalContext _context;

    public readonly struct NavigationParameter
    {
        public string[] Arguments { get; init; }
    }

    private readonly IParameterViewStackService _viewStackService;
    public string Id { get; } = nameof(PreparationViewModel);

    public PreparationViewModel(
        IParameterViewStackService viewStackService,
        IPath path,
        IFile file,
        IGlobalContext context
    )
    {
        _viewStackService = viewStackService;
        _path = path;
        _file = file;
        _context = context;
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
        await Task.Delay(10).ConfigureAwait(true);

        try
        {
            if (arguments.Length == 0)
            {
                NavigateToGettingStarted();

                return;
            }

            _context.FilePath = ParseArguments(arguments);

            _viewStackService
                .PushPage<TableViewModel>(new Sextant.NavigationParameter(), resetStack: true)
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

    private void NavigateToGettingStarted()
    {
        //_viewStackService.PushPage<GettingStartedModelView>().Subscribe();
    }

    private string ParseArguments(string[] arguments)
    {
        if (arguments.Length > 1)
        {
            throw new Exception(
                "Too many arguments passed in. The first argument must be the file path to the debian package."
            );
        }

        var filePath = arguments[0];

        if (!_file.Exists(filePath))
        {
            throw new FileNotFoundException("The file doesn't exist", filePath);
        }

        return _path.GetFullPath(filePath);
    }
}
