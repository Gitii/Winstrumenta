using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.Hosting;
using PackageInstaller.Core.Helpers;
using ReactiveUI;
using Sextant;

namespace PackageInstaller.Core.ModelViews;

public class ResultViewModel : ReactiveObject, IViewModel, INavigable
{
    private readonly IHostApplicationLifetime _applicationLifetime;

    public enum ResultTheme
    {
        Success,
        Warning,
        Error,
    }

    public readonly struct NavigationParameter
    {
        public string Title { get; init; }
        public string Description { get; init; }
        public string? Details { get; init; }

        public ResultTheme? Theme { get; init; }
    }

    public ResultViewModel(IHostApplicationLifetime applicationLifetime)
    {
        _applicationLifetime = applicationLifetime;

        CloseCommand = ReactiveCommand.Create(
            () =>
            {
                _applicationLifetime.StopApplication();
                Environment.Exit(1);
            }
        );

        ToggleDetailsCommand = ReactiveCommand.Create(
            () => { AreDetailsVisible = !AreDetailsVisible; }
        );
    }

    public string Id { get; } = nameof(ResultViewModel);

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    private string _title;
    private string _description;
    string _details;
    bool _areDetailsVisible;
    ResultTheme _theme;

    public ResultTheme Theme
    {
        get { return _theme; }
        set { this.RaiseAndSetIfChanged(ref _theme, value); }
    }

    public bool AreDetailsVisible
    {
        get { return _areDetailsVisible; }
        set { this.RaiseAndSetIfChanged(ref _areDetailsVisible, value); }
    }

    public string Details
    {
        get { return _details; }
        set { this.RaiseAndSetIfChanged(ref _details, value); }
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

        Title = navParms.Title;
        Description = navParms.Description;
        Details = navParms.Details ?? String.Empty;

        AreDetailsVisible = false;

        return Observable.Return(Unit.Default);
    }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    public ReactiveCommand<Unit, Unit> ToggleDetailsCommand { get; }
}