using System;
using System.Reflection;
using System.Threading.Tasks;
using Community.Wsl.Sdk.Strategies.Api;
using CommunityToolkit.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PackageInstaller.Core;
using PackageInstaller.Core.ModelViews;
using PackageInstaller.Core.Services;
using PackageInstaller.Pages;
using ReactiveUI;
using Sextant;
using Sextant.WinUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace PackageInstaller
{
    public static class Program
    {
        [STAThread]
        public static Task Main(string[] args)
        {
            var builder = new WindowsAppSdkHostBuilder<App>();

            builder.ConfigureServices(
                (context, collection) =>
                {
                    collection.UseMicrosoftDependencyResolver();
                    var resolver = Locator.CurrentMutable;
                    resolver.InitializeSplat();
                    resolver.InitializeReactiveUI(RegistrationNamespace.WinUI);

                    Locator.CurrentMutable.RegisterViewsForViewModels(
                        Assembly.GetCallingAssembly()
                    );
                    Locator.CurrentMutable
                        .RegisterWinUIViewLocator()
                        .RegisterNavigationView(
                            () =>
                                new NavigationView(
                                    RxApp.MainThreadScheduler,
                                    RxApp.TaskpoolScheduler,
                                    ViewLocator.Current
                                )
                        )
                        .RegisterParameterViewStackService()
                        .RegisterViewWinUI<PackageActions, PackageActionsViewModel>()
                        .RegisterViewWinUI<Error, ErrorViewModel>()
                        .RegisterViewWinUI<Preparation, PreparationViewModel>();

                    collection.AddSingleton<MainWindow>();
                    collection.AddTransient<InstallViewModel>();
                    collection.AddTransient<ErrorViewModel>();
                    collection.AddTransient<PackageActionsViewModel>();
                    collection.AddTransient<PreparationViewModel>();
                    collection.AddTransient<IDebianPackageReader, DebianPackageReader>();
                    collection.AddTransient<IPackageReader, PackageReader>();
                    collection.AddTransient<IWsl, WslImpl>();
                    collection.AddTransient<IWslApi, ComBasedWslApi>();
                    collection.AddTransient<IPackageManager, PackageManager>();
                    collection.AddTransient<IDpkg, Dpkg>();
                    //collection.RemoveAll<IActivationForViewFetcher>();
                    //collection.AddTransient<IActivationForViewFetcher, ActivationForViewFetcher>();
                }
            );

            var app = builder.Build();

            Container = app.Services;
            Container.UseMicrosoftDependencyResolver();

            return app.StartAsync();
        }

        public static IServiceProvider Container { get; private set; }
    }
}
