using System;
using System.Drawing.Design;
using System.Reflection;
using System.Threading.Tasks;
using Community.Sextant.WinUI.Microsoft.Extensions.DependencyInjection;
using Community.Wsa.Sdk.Strategies.Api;
using Community.Wsa.Sdk.Strategies.Packages;
using Community.Wsl.Sdk.Strategies.Api;
using CommunityToolkit.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PackageInstaller.Core.ModelViews;
using PackageInstaller.Core.Services;
using PackageInstaller.IconThemes;
using PackageInstaller.Pages;
using PackageInstaller.Themes;
using ReactiveUI;
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
                ConfigureModelViews(collection);
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
                .FromAssembliesOf(typeof(IDistributionProvider), typeof(WslProvider))
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
        collection.AddSingleton<ThemeManager>();
        collection.AddSingleton<IThemeManager, ThemeManager>(
            provider => provider.GetRequiredService<ThemeManager>()
        );
        collection.AddTransient<IWslApi, ManagedWslApi>();
        collection.AddTransient<IWsaApi, WsaApi>();
        collection.AddTransient<IPackageManager, ManagedPackageManager>();
        collection.AddSingleton<IIconThemeManager, IconThemeManager>();
        collection.AddSingleton<IThreadHelpers>(
            (provider) =>
            {
                var mw = provider.GetRequiredService<MainWindow>();

                return new ThreadHelpers(mw.DispatcherQueue);
            }
        );
    }

    private static void ConfigureModelViews(IServiceCollection collection)
    {
        collection.UseSextant(
            builder =>
            {
                builder.ConfigureDefaults();
                builder.ConfigureViews(
                    viewBuilder =>
                    {
                        viewBuilder
                            .RegisterViewAndViewModel<PackageActions, PackageActionsViewModel>()
                            .RegisterViewAndViewModel<Error, ErrorViewModel>()
                            .RegisterViewAndViewModel<Result, ResultViewModel>()
                            .RegisterViewAndViewModel<Preparation, PreparationViewModel>();
                    }
                );
            }
        );
    }

    private static void ConfigureSplatIntegration(IServiceCollection collection)
    {
        collection.UseMicrosoftDependencyResolver();
        var resolver = Locator.CurrentMutable;
        resolver.InitializeSplat();
        resolver.InitializeReactiveUI(RegistrationNamespace.WinUI);
    }
}
