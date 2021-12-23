using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using PackageInstaller.Core.Helpers;
using PackageInstaller.Core.Services;
using ReactiveUI;
using Sextant;

namespace PackageInstaller.Core.ModelViews
{
    public class PreparationViewModel : ReactiveObject, IViewModel, INavigable
    {
        private IPackageReader _packageReader;

        public readonly struct NavigationParameter
        {
            public string[] Arguments { get; init; }
        }

        private readonly IParameterViewStackService _viewStackService;
        public string Id { get; } = nameof(PreparationViewModel);

        public PreparationViewModel(
            IParameterViewStackService viewStackService,
            IPackageReader packageReader
        )
        {
            _viewStackService = viewStackService;
            _packageReader = packageReader;
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

            return Process(navParms.Arguments).ToObservable(RxApp.MainThreadScheduler);
        }

        public async Task Process(string[] arguments)
        {
            await Task.Delay(100);

            try
            {
                string packageFilePath = ParseArguments(arguments);

                (var isSupported, string? reason) = await _packageReader.IsSupported(packageFilePath);

                if (!isSupported)
                {
                    throw new Exception($"File is not a valid package: {reason}");
                }

                var data = await _packageReader.ReadMetaData(packageFilePath);


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

        private string ParseArguments(string[] arguments)
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

            return filePath;
        }
    }
}
