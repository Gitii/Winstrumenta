using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;
using PackageInstaller.Core.ModelViews;
using ReactiveUI;

namespace PackageInstaller.Pages;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "MA0048:File name must match type name",
    Justification = "Workaround: Xaml doesn't support generic types."
)]
public class ReactivePageNotificationHub : ReactivePage<NotificationHubModelView> { }

public sealed partial class NotificationHub : ReactivePageNotificationHub
{
    public NotificationHub()
    {
        this.InitializeComponent();

        this.WhenActivated(
            (disposable) =>
            {
                this.OneWayBind(ViewModel, (mv) => mv.Notifications, (mv) => mv.List.ItemsSource)
                    .DisposeWith(disposable);

                Observable
                    .FromEventPattern<
                        TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs>,
                        NavigationViewBackRequestedEventArgs
                    >(
                        ev => this.NavigationView.BackRequested += ev,
                        ev => this.NavigationView.BackRequested -= ev
                    )
                    .Select(x => Unit.Default)
                    .InvokeCommand(ViewModel!.GoBackCommand)
                    .DisposeWith(disposable);
            }
        );
    }
}
