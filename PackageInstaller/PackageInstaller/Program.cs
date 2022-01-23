using System;
using System.Diagnostics.CodeAnalysis;
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


namespace PackageInstaller;

public static class Program
{
    [STAThread]
    public static Task Main(string[] args)
    {
        var builder = new WindowsAppSdkHostBuilder<App>();

        ConfigureServices(builder);

        var app = builder.Build();

        app.Services.UseMicrosoftDependencyResolver();

        return app.StartAsync();
    }

    private static void ConfigureServices(WindowsAppSdkHostBuilder<App> builder)
    {
        builder.ConfigureServices(
            (context, collection) =>
            {
                ConfigureSplatIntegration(collection);
                ConfigureModelViews(Locator.CurrentMutable);
                ConfigureComplexServices(collection);
                ConfigureServiceDiscovery(collection);
            }
        );
    }

    private static void ConfigureServiceDiscovery(IServiceCollection collection)
    {
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
    }

    private static void ConfigureComplexServices(IServiceCollection collection)
    {
        collection.AddSingleton<MainWindow>();
        var themeManager = new ThemeManager();
        collection.AddSingleton<IThemeManager, ThemeManager>(provider => themeManager);
        collection.AddSingleton<ThemeManager>(provider => themeManager);
        collection.AddTransient<IWslApi, ManagedWslApi>();
        collection.AddSingleton<IIconThemeManager, IconThemeManager>();
        collection.AddSingleton<IThreadHelpers>(
            (provider) =>
            {
                var mw = provider.GetRequiredService<MainWindow>();

                return new ThreadHelpers(mw.DispatcherQueue);
            }
        );
    }

    private static void ConfigureModelViews(IMutableDependencyResolver currentMutable)
    {
        currentMutable.RegisterViewsForViewModels(
            Assembly.GetCallingAssembly()
        );
        currentMutable
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
    }

    private static void ConfigureSplatIntegration(IServiceCollection collection)
    {
        collection.UseMicrosoftDependencyResolver();
        var resolver = Locator.CurrentMutable;
        resolver.InitializeSplat();
        resolver.InitializeReactiveUI(RegistrationNamespace.WinUI);
    }
}
