using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace PackageInstaller.Core.Helpers
{
    public static class ObservableAsync
    {
        /// <summary>
        /// Converts an asynchronous action into an observable sequence. Each subscription to the resulting sequence causes the action to be started.
        /// </summary>
        /// <param name="actionAsync">Asynchronous action to convert.</param>
        /// <returns>An observable sequence exposing a Unit value upon completion of the action, or an exception.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="actionAsync"/> is null.</exception>
        public static IObservable<Unit> FromAsync(Func<Task> actionAsync)
        {
            if (actionAsync == null)
            {
                throw new ArgumentNullException(nameof(actionAsync));
            }

            return Observable.FromAsync(async () =>
            {
                try
                {
                    await actionAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    RxApp.DefaultExceptionHandler.OnNext(e);
                }
            });
        }
    }
}