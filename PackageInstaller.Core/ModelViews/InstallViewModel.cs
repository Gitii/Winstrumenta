using System.Reactive;
using System.Reactive.Linq;
using PackageInstaller.Core.Helpers;
using ReactiveUI;
using Sextant;

namespace PackageInstaller.Core.ModelViews;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "MA0004:Use Task.ConfigureAwait(false)",
    Justification = "ModelView should care about thread context."
)]
public class InstallViewModel : ReactiveObject, Sextant.IViewModel, INavigable
{
    public struct NavigationParameter
    {
        public string[] Arguments { get; set; }
    }

    public string Id => nameof(InstallViewModel);

    public IObservable<Unit> WhenNavigatedFrom(INavigationParameter parameter)
    {
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> WhenNavigatedTo(INavigationParameter parameter)
    {
        var parms = parameter.FromNavigationParameter<NavigationParameter>();

        if (parms.Arguments.Length == 0)
        {
            throw new Exception($"No arguments");
        }

        var package = parms.Arguments[0];

        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> WhenNavigatingTo(INavigationParameter parameter)
    {
        return Observable.Return(Unit.Default);
    }
}
