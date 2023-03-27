using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using Windows.System;
using Community.Sextant.WinUI;
using Community.Sextant.WinUI.Adapters;
using DynamicData;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using NativeInterop.Win32.Xaml;
using Numbers.Core.ModelViews;
using Numbers.Core.Services;
using ReactiveUI;
using Sextant;
using Shared.Misc;
using Shared.Services;

namespace Numbers;

public sealed partial class MainWindow : DesktopWindow
{
    private const string DEFAULT_APP_TITLE = "Numbers";
    private const string DEFAULT_NO_TITLE = "<No title>";
    private readonly IParameterViewStackService _viewStackService;
    private readonly INavigationService _navigationService;
    private readonly IApplicationLifeCycle _lifeCycle;
    private string args = string.Empty;
    private IDisposable? _windowUiDisposable;

    public MainWindow(
        IParameterViewStackService viewStackService,
        INavigationService navigationService,
        IApplicationLifeCycle lifeCycle
    )
    {
        _viewStackService = viewStackService;
        _navigationService = navigationService;
        _lifeCycle = lifeCycle;

        this.InitializeComponent();

        Title = AppTitle.Text = DEFAULT_APP_TITLE;
        ExtendsContentIntoTitleBar = true; // enable custom titlebar
        SetTitleBar(AppTitleBar); // set user ui element as titlebar
    }

    public void SetLaunchArgs(string arguments)
    {
        this.args = arguments;
    }

    private void MainWindow_OnActivated(object sender, WindowLoadedEventArgs args)
    {
        var uiContext =
            SynchronizationContext.Current ?? throw new Exception("UI Context is null!");

        RxApp.DefaultExceptionHandler = Observer
            .Create<Exception>(
                (ex) =>
                {
                    uiContext.Post(
                        (_) =>
                        {
                            var navParms = new ErrorViewModel.NavigationParameter()
                            {
                                Exception = ex
                            };

                            _viewStackService
                                .PushPage<ErrorViewModel>(navParms.ToNavigationParameter())
                                .Subscribe();
                        },
                        null
                    );
                }
            )
            .NotifyOn(RxApp.MainThreadScheduler);

        _navigationService.SetAdapter(new FrameNavigationViewAdapter(Content, this));

        ForceUpdateTheme();
        _viewStackService.PageStack.Subscribe(OnPageChanged);

        var arguments = Environment.GetCommandLineArgs().Skip(1).ToArray();
        if (arguments.Length == 0)
        {
            var arg0 = Environment.GetEnvironmentVariable("ARG0");
            if (arg0 != null)
            {
                arguments = new[] { arg0 };
            }
        }

        if (arguments.Length == 0)
        {
            _viewStackService.PushPage<StartViewModel>(new NavigationParameter()).Subscribe();
        }
        else
        {
            var navParams = new PreparationViewModel.NavigationParameter()
            {
                Arguments = arguments,
            };

            _viewStackService
                .PushPage<PreparationViewModel>(navParams.ToNavigationParameter())
                .Subscribe();
        }
    }

    private void OnPageChanged(IImmutableList<IViewModel> stack)
    {
        ResetWindow();
        if (stack.Count == 0)
        {
            return;
        }

        if (stack.Last() is IWindowInterface ui)
        {
            if (_windowUiDisposable != null)
            {
                _windowUiDisposable.Dispose();
                _windowUiDisposable = null;
            }

            var disposable = new CompositeDisposable();

            ui.WhenAnyValue(x => x.WindowTitle)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(
                    (title) =>
                    {
                        Title = AppTitle.Text = title ?? DEFAULT_NO_TITLE;
                    }
                )
                .DisposeWith(disposable);

            ui.WhenAnyValue((x) => x.Commands)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(
                    (items) =>
                    {
                        CommandBar.Visibility =
                            items?.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
                        CommandBar.PrimaryCommands.Clear();
                        CommandBar.PrimaryCommands.AddRange(
                            items?.Select(ToAppBarButton) ?? Array.Empty<AppBarButton>()
                        );
                    }
                )
                .DisposeWith(disposable);

            _windowUiDisposable = disposable;
        }
    }

    private AppBarButton ToAppBarButton(CommandDescription arg)
    {
        return (
            new AppBarButton()
            {
                Label = arg.Label,
                Icon =
                    arg.Icon != null
                        ? new FontIcon()
                          {
                              FontFamily = new FontFamily("Segoe MDL2 Assets"),
                              Glyph = arg.Icon
                          }
                        : null,
                KeyboardAccelerators =
                {
                    new KeyboardAccelerator()
                    {
                        Key = Enum.Parse<VirtualKey>(
                            arg.KeyboardAcceleratorKey ?? VirtualKey.None.ToString()
                        ),
                        IsEnabled = !string.IsNullOrEmpty(arg.KeyboardAcceleratorKey),
                        Modifiers = Enum.Parse<VirtualKeyModifiers>(
                            arg.KeyboardAcceleratorModifier ?? VirtualKeyModifiers.None.ToString()
                        )
                    }
                },
                Command = arg.Command
            }
        );
    }

    private void ResetWindow()
    {
        if (_windowUiDisposable != null)
        {
            _windowUiDisposable.Dispose();
            _windowUiDisposable = null;
        }

        CommandBar.PrimaryCommands.Clear();
        CommandBar.SecondaryCommands.Clear();
    }

    private void MainWindow_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _lifeCycle.Exit(0);
    }
}
