using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.Hosting;
using PackageInstaller.Core.Helpers;
using ReactiveUI;
using Sextant;

namespace PackageInstaller.Core.ModelViews
{
    public class ErrorViewModel : ReactiveObject, IViewModel, INavigable
    {
        private readonly IHostApplicationLifetime _applicationLifetime;

        public readonly struct NavigationParameter
        {
            public readonly Exception Exception { get; init; }
        }

        public ErrorViewModel(IHostApplicationLifetime applicationLifetime)
        {
            _applicationLifetime = applicationLifetime;

            Close = ReactiveCommand.Create(
                () =>
                {
                    _applicationLifetime.StopApplication();
                    Environment.Exit(1);
                }
            );
        }

        public string Id { get; } = nameof(ErrorViewModel);

        public string ErrorTitle
        {
            get => _errorTitle;
            set => this.RaiseAndSetIfChanged(ref _errorTitle, value);
        }

        public string ErrorDescription
        {
            get => _errorDescription;
            set => this.RaiseAndSetIfChanged(ref _errorDescription, value);
        }

        private string _errorTitle;
        private string _errorDescription;

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

            ErrorTitle = "An unhandled exception occurred";
            ErrorDescription = navParms.Exception.ToString();

            return Observable.Return(Unit.Default);
        }

        public ReactiveCommand<Unit, Unit> Close { get; }
    }
}
