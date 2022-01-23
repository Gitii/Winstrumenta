using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using System.Reactive;
using System.Threading;
using NativeInterop.Win32.Xaml;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Splat;
using Sextant.WinUI;
using Sextant;
using PackageInstaller.Core.Helpers;
using PackageInstaller.Core.ModelViews;
using PackageInstaller.Core.Services;
using PackageInstaller.Themes;

namespace PackageInstaller;

public sealed partial class MainWindow : DesktopWindow
{
    private readonly IServiceProvider services;
    private string args = string.Empty;

    public MainWindow(IServiceProvider services)
    {
        this.InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        // SetTitleBar(TitleBar); // do not set the title bar to use a 100% custom one.

        this.services = services;
    }

    public void SetLaunchArgs(string arguments)
    {
        this.args = arguments;
    }

    private void MainWindow_OnActivated(object sender, WindowLoadedEventArgs args)
    {
        var uiContext =
            SynchronizationContext.Current ?? throw new Exception("UI Context is null!");

        var stackService = this.services.GetRequiredService<IParameterViewStackService>();

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

                            stackService
                                .PushPage<ErrorViewModel>(navParms.ToNavigationParameter())
                                .Subscribe();
                        },
                        null
                    );
                }
            )
            .NotifyOn(RxApp.MainThreadScheduler);

        Sextant.WinUI.NavigationView navigationView = Locator.Current.GetNavigationView(
            "NavigationView"
        )!;
        navigationView.ShowDefaultBackButton = false;
        ContentControl.Content = navigationView;

        Locator.Current.GetService<ThemeManager>()!.SetPanel(TitleBar);

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

        stackService.PushPage<PreparationViewModel>(navParams.ToNavigationParameter()).Subscribe();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    private void TitleBar_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _nativeWindow.StartDragging();
        e.Handled = true;
    }
}
