using System;
using System.Reflection;
using System.Threading.Tasks;
using Community.Wsl.Sdk.Strategies.Api;
using CommunityToolkit.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PackageInstaller.Core.ModelViews;
using PackageInstaller.Core.Services;
using PackageInstaller.IconThemes;
using PackageInstaller.Pages;
using PackageInstaller.Themes;
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
                        .RegisterViewWinUI<Result, ResultViewModel>()
                        .RegisterViewWinUI<Preparation, PreparationViewModel>();

                    collection.AddSingleton<MainWindow>();
                    //collection.AddTransient<InstallViewModel>();
                    //collection.AddTransient<ErrorViewModel>();
                    //collection.AddTransient<PackageActionsViewModel>();
                    //collection.AddTransient<PreparationViewModel>();
                    //collection.AddTransient<IDebianPackageReader, DebianPackageReader>();
                    //collection.AddTransient<IPackageReader, PackageReader>();
                    //collection.AddTransient<IWsl, WslImpl>();
                    var themeManager = new ThemeManager();
                    collection.AddSingleton<IThemeManager, ThemeManager>(provider => themeManager);
                    collection.AddSingleton<ThemeManager>(provider => themeManager);
                    collection.AddTransient<IWslApi, ManagedWslApi>();
                    collection.AddSingleton<IIconThemeManager, IconThemeManager>();
                    //collection.AddTransient<IPackageManager, PackageManager>();
                    //collection.AddTransient<IDpkg, Dpkg>();
                    collection.Scan(
                        scan =>
                            scan
                            // We start out with all types in the assembly of ITransientService
                            .FromAssembliesOf(typeof(IWsl), typeof(WslImpl))
                                .AddClasses(true)
                                .AsImplementedInterfaces()
                                .WithTransientLifetime()
                                .AddClasses((classes) => classes.AssignableTo<ReactiveObject>())
                                .AsSelf()
                    );
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

        public static MainWindow? MainWindow
        {
            get => Container.GetService<MainWindow>();
        }
    }
}
