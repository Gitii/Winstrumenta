using System;
using Community.Sextant.WinUI.Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Numbers.Core.ModelViews;
using Numbers.Core.Services;
using Numbers.Pages;
using ReactiveUI;
using Shared.Services;
using Shared.Services.Implementations;
using Shared.Services.Implementations.WinUI;
using Shared.Services.Implementations.WinuiUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace Numbers;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = new WindowsAppSdkHostBuilder<App>();

        ConfigureServices(builder);

        var app = builder.Build();

        app.Services.UseMicrosoftDependencyResolver();

        app.StartAsync().GetAwaiter().GetResult();
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
                        typeof(IFile), // interfaces of services
                        typeof(ICsv), // interfaces of services
                        typeof(Csv), // non-ui implementations of services
                        typeof(FileImpl), // non-ui implementations of services
                        typeof(Launcher) // WinUI implementations of services
                    )
                    .AddClasses(true)
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .WithTransientLifetime()
                    .AddClasses((classes) => classes.AssignableTo<ReactiveObject>())
                    .AsSelf()
        );

        collection.AddSingleton<IGlobalContext, GlobalContext>();
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
                        viewBuilder.RegisterViewAndViewModel<Preparation, PreparationViewModel>();
                        viewBuilder.RegisterViewAndViewModel<Table, TableViewModel>();
                        viewBuilder.RegisterViewAndViewModel<Error, ErrorViewModel>();
                        viewBuilder.RegisterViewAndViewModel<Save, SaveViewModel>();
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
