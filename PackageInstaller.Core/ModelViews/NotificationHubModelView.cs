using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using PackageInstaller.Core.Helpers;
using ReactiveUI;
using Sextant;

namespace PackageInstaller.Core.ModelViews;

public class NotificationHubModelView : ReactiveObject, IViewModel, INavigable
{
    private readonly IParameterViewStackService _stackService;

    public string Id { get; } = nameof(NotificationHubModelView);

    public NotificationHubModelView(IParameterViewStackService stackService)
    {
        _stackService = stackService;
        _notifications = ImmutableList.Create<NotificationModelView>();

        GoBackCommand = ReactiveCommand.Create<Unit, Unit>(
            (_) =>
            {
                stackService.PopPage().Subscribe();
                return Unit.Default;
            }
        );
    }

    public readonly struct NavigationParameter
    {
        public IImmutableList<NotificationModelView> Notifications { get; init; }
    }

    IImmutableList<NotificationModelView> _notifications;

    public IImmutableList<NotificationModelView> Notifications
    {
        get { return _notifications; }
        set { this.RaiseAndSetIfChanged(ref _notifications, value); }
    }

    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }

    public IObservable<Unit> WhenNavigatedTo(INavigationParameter parameter)
    {
        var navParams = parameter.FromNavigationParameter<NavigationParameter>();

        Notifications = navParams.Notifications;

        return Observable.Empty<Unit>();
    }

    public IObservable<Unit> WhenNavigatedFrom(INavigationParameter parameter)
    {
        return Observable.Empty<Unit>();
    }

    public IObservable<Unit> WhenNavigatingTo(INavigationParameter parameter)
    {
        return Observable.Empty<Unit>();
    }
}
