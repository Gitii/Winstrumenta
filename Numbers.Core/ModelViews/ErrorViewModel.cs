#region

using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Sextant;
using Shared.Misc;
using Shared.Services;

#endregion

namespace Numbers.Core.ModelViews;

public class ErrorViewModel : ReactiveObject, IViewModel, INavigable
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IApplicationLifeCycle _lifeCycle;
    private string _errorDescription;
    string _errorDetails;
    bool _errorDetailsVisible;
    private string _errorTitle;

    public ErrorViewModel(
        IHostApplicationLifetime applicationLifetime,
        IApplicationLifeCycle lifeCycle
    )
    {
        _applicationLifetime = applicationLifetime;
        _lifeCycle = lifeCycle;

        _errorDescription = String.Empty;
        ErrorDescription = String.Empty;
        _errorDetails = String.Empty;
        ErrorDetails = String.Empty;
        _errorDetailsVisible = false;
        ErrorDetailsVisible = false;
        _errorTitle = String.Empty;
        ErrorTitle = String.Empty;

        Close = ReactiveCommand.Create(
            () =>
            {
                _applicationLifetime.StopApplication();
                _lifeCycle.Exit(1);
            }
        );

        ToggleErrorDetails = ReactiveCommand.Create(
            () =>
            {
                ErrorDetailsVisible = !ErrorDetailsVisible;
            }
        );
    }

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

    public bool ErrorDetailsVisible
    {
        get { return _errorDetailsVisible; }
        set { this.RaiseAndSetIfChanged(ref _errorDetailsVisible, value); }
    }

    public string ErrorDetails
    {
        get { return _errorDetails; }
        set { this.RaiseAndSetIfChanged(ref _errorDetails, value); }
    }

    public ReactiveCommand<Unit, Unit> Close { get; }

    public ReactiveCommand<Unit, Unit> ToggleErrorDetails { get; }

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

        if (
            navParms.Exception.Message.Contains(
                "RPC_E_WRONG_THREAD",
                StringComparison.InvariantCulture
            )
        )
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        ErrorTitle = "An unhandled exception occurred";
        ErrorDescription = navParms.Exception.Message;
        ErrorDetails = navParms.Exception is DetailedException dex
            ? dex.Details
            : navParms.Exception.ToString();

        ErrorDetailsVisible = false;

        return Observable.Return(Unit.Default);
    }

    public string Id { get; } = nameof(ErrorViewModel);

    public readonly struct NavigationParameter
    {
        public readonly Exception Exception { get; init; }
    }
}
