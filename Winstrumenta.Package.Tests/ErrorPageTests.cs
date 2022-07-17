using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Appium.Windows;

namespace Winstrumenta.Package.Tests;

public class ErrorPageTests
{
    private WindowsDriver<WindowsElement> _session = null!;
    private TestEnvironment _environment = null!;

    [SetUp]
    public async Task SetupAsync()
    {
        _environment = new TestEnvironment(TestContext.CurrentContext.TestDirectory);
        var d = await _environment.PrepareAsync().ConfigureAwait(true);

        var opts = new AppiumOptions() { PlatformName = "windows", };
        //opts.AddAdditionalCapability("app", "");
        //opts.AddAdditionalCapability("automationName", "windows");
        opts.AddAdditionalCapability(
            "app",
            "56395Gitii.Winstrumenta.PackageManager_qkrp4w1fft63e!App"
        );
        //opts.AddAdditionalCapability("deviceName", "WindowsPC");
        //opts.AddAdditionalCapability("ms:experimental-webdriver", true);

        Environment.SetEnvironmentVariable("APPIUM_WAD_PATH", d.WadPath);

        //var builder = new AppiumServiceBuilder()
        //    .UsingDriverExecutable(new FileInfo(d.NodePath))
        //    .WithAppiumJS(new FileInfo(d.AppiumPath))
        //    .WithEnvironment(new Dictionary<string, string>() { { "APPIUM_WAD_PATH", d.WadPath } })
        //    .UsingPort(4000);

        await _environment.StartAsync().ConfigureAwait(true);

        _session = new WindowsDriver<WindowsElement>(
            new Uri($"http://{TestEnvironment.WAD_URL}"),
            opts
        );
    }

    [TearDown]
    public Task TeardownAsync()
    {
        _session?.Quit();
        _session = null;

        return _environment.TeardownAsync();
    }

    [Test]
    public void Test1()
    {
        _session.FindElementByName("Close");
    }
}
