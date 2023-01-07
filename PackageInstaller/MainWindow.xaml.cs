using Microsoft.UI.Xaml;
using System;
using System.Linq;
using System.Reactive;
using System.Threading;
using Community.Sextant.WinUI;
using Community.Sextant.WinUI.Adapters;
using NativeInterop.Win32.Xaml;
using ReactiveUI;
using Microsoft.UI.Xaml.Controls;
using Sextant;
using PackageInstaller.Core.ModelViews;
using PackageInstaller.Core.Services.WinUI;
using Shared.Misc;

namespace PackageInstaller;

public sealed partial class MainWindow : DesktopWindow
{
    private readonly IParameterViewStackService _viewStackService;
    private readonly INavigationService _navigationService;
    private readonly ThemeManager _themeManager;
    private string args = string.Empty;

    public MainWindow(
        IParameterViewStackService viewStackService,
        INavigationService navigationService,
        ThemeManager themeManager
    )
    {
        _viewStackService = viewStackService;
        _navigationService = navigationService;
        _themeManager = themeManager;

        this.InitializeComponent();

        Title = "Winstrumenta PackageManager";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(DummyTitleBar); // do not set the title bar to use a 100% custom one.
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

        var content = new Frame();

        _navigationService.SetAdapter(new FrameNavigationViewAdapter(content, this));

        ContentControl.Content = content;

        _themeManager.SetPanel(DummyTitleBar);
        ForceUpdateTheme();

        var arguments = Environment.GetCommandLineArgs().Skip(1).ToArray();
        if (arguments.Length == 0)
        {
            var arg0 = Environment.GetEnvironmentVariable("ARG0");
            if (arg0 != null)
            {
                arguments = new[] { arg0 };
            }
        }

        var navParams = new PreparationViewModel.NavigationParameter() { Arguments = arguments, };

        _viewStackService
            .PushPage<PreparationViewModel>(navParams.ToNavigationParameter())
            .Subscribe();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }
}
