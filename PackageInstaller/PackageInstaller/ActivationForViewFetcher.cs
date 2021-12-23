using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using ReactiveUI;

namespace PackageInstaller
{
    public class ActivationForViewFetcher : IActivationForViewFetcher
    {
        /// <inheritdoc/>
        public int GetAffinityForView(Type view) =>
            typeof(FrameworkElement).GetTypeInfo().IsAssignableFrom(view.GetTypeInfo()) ? 10 : 0;

        /// <inheritdoc/>
        public IObservable<bool> GetActivationForView(IActivatableView view)
        {
            if (view is not FrameworkElement fe)
            {
                return Observable.Empty<bool>();
            }

            var viewIsLoaded = fe.WhenAny((x) => x.IsLoaded, (x) => x);

            return viewIsLoaded
                .Select(
                    b =>
                        b.Value
                            ? fe.WhenAnyValue(x => x.IsHitTestVisible).SkipWhile(x => !x)
                            : Observable.Return(false)
                )
                .Switch()
                .DistinctUntilChanged();
        }
    }
}
