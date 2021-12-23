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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using NativeInterop.Win32;
using NativeInterop.Win32.Xaml;
using PackageInstaller.Core;
using PackageInstaller.Pages;
using ReactiveUI;
using Vanara.PInvoke;
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

            _nativeWindow.SetTitlebarBackgroundColor(Color.Black);
            this.services = services;
            //ExtendsContentIntoTitleBar = true;
        }

        public void SetLaunchArgs(string arguments)
        {
            this.args = arguments;
        }

        private void MainWindow_OnActivated(object sender, WindowLoadedEventArgs args)
        {
            Sextant.WinUI.NavigationView? navigationView = Locator.Current.GetNavigationView(
                "NavigationView"
            );
            navigationView.ShowDefaultBackButton = false;
            Content = navigationView;

            var navParams = new PreparationViewModel.NavigationParameter()
            {
                Arguments = Environment.GetCommandLineArgs().Skip(1).ToArray(),
            };

            this.services
                .GetRequiredService<IParameterViewStackService>()
                .PushPage<PreparationViewModel>(navParams.ToNavigationParameter())
                .Subscribe();
        }
    }
}
