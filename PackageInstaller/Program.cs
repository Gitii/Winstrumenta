using System;
using System.Threading.Tasks;
using Community.Sextant.WinUI.Microsoft.Extensions.DependencyInjection;
using Community.Wsa.Sdk;
using Community.Wsl.Sdk;
using CommunityToolkit.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PackageInstaller.Core.ModelViews;
using PackageInstaller.Core.Services;
using PackageInstaller.Core.Services.WinUI;
using PackageInstaller.Pages;
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
                .FromAssembliesOf(
                        typeof(IDistributionProvider),
                        typeof(WslProvider),
                        typeof(IconThemeManager)
                    )
                    .AddClasses(true)
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .WithTransientLifetime()
                    .AddClasses((classes) => classes.AssignableTo<ReactiveObject>())
                    .AsSelf()
        );

        collection.Scan(
            scan =>
                scan
                // We start out with all types in the assembly of ITransientService
                .FromAssembliesOf(typeof(IWsaApi), typeof(IWslApi))
                    .AddClasses(true)
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .WithTransientLifetime()
        );
    }

    private static void ConfigureComplexServices(IServiceCollection collection)
    {
        collection.AddSingleton<MainWindow>();
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
                            .RegisterViewAndViewModel<NotificationHub, NotificationHubModelView>()
                            .RegisterViewAndViewModel<PackageActions, PackageActionsViewModel>()
                            .RegisterViewAndViewModel<Error, ErrorViewModel>()
                            .RegisterViewAndViewModel<Result, ResultViewModel>()
                            .RegisterViewAndViewModel<ActionExecution, ActionExecutionViewModel>()
                            .RegisterViewAndViewModel<GettingStarted, GettingStartedModelView>()
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
