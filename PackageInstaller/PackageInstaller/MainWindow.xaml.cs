using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using NativeInterop.Win32;
using NativeInterop.Win32.Xaml;
using PackageInstaller.Core;
using PackageInstaller.Pages;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Splat;
using Sextant.WinUI;
using Sextant;
using PackageInstaller.Core.Helpers;
using PackageInstaller.Core.ModelViews;

namespace PackageInstaller
{
    public sealed partial class MainWindow : DesktopWindow
    {
        private readonly IServiceProvider services;
        private string args = string.Empty;

        public MainWindow(IServiceProvider services)
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            // SetTitleBar(TitleBar);

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

            Sextant.WinUI.NavigationView? navigationView = Locator.Current.GetNavigationView(
                "NavigationView"
            );
            navigationView.ShowDefaultBackButton = false;
            ContentControl.Content = navigationView;
            ForceUpdateTheme();

            var navParams = new PreparationViewModel.NavigationParameter()
            {
                Arguments = Environment.GetCommandLineArgs().Skip(1).ToArray(),
            };

            stackService
                .PushPage<PreparationViewModel>(navParams.ToNavigationParameter())
                .Subscribe();
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
}
