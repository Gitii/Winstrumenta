namespace Winstrumenta.Package.Tests.HostEnvironment;

internal interface IHostEnvironment
{
    Task StartAsync();
    Task StopAsync();
    Task ResetAsync();
}
