using System.Diagnostics;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace Winstrumenta.Package.Tests;

public class TestSuiteBase
{
    private WindowsDriver<WindowsElement>? _session = null!;
    private TestEnvironment? _environment = null!;

    [OneTimeSetUp]
    public virtual async Task GlobalSetupAsync()
    {
        var env = new TestEnvironment(TestContext.CurrentContext.TestDirectory);
        _environment = env;

        var d = await env.PrepareAsync().ConfigureAwait(true);

        Environment.SetEnvironmentVariable("APPIUM_WAD_PATH", d.WadPath);

        await _environment.StartAsync().ConfigureAwait(true);
    }

    [SetUp]
    public virtual void Setup()
    {
        CreateSession(Array.Empty<string>());
    }

    private void CreateSession(string[] arguments)
    {
        DestroySession();

        var opts = new AppiumOptions() { PlatformName = "windows", };

        opts.AddAdditionalCapability(
            "app",
            "56395Gitii.Winstrumenta.PackageManager_qkrp4w1fft63e!App"
        );
        opts.AddAdditionalCapability("appArguments", ToArgumentString(arguments));
        opts.AddAdditionalCapability("deviceName", "WindowsPC");

        _session = new WindowsDriver<WindowsElement>(
            new Uri($"{TestEnvironment.WAD_URL}/wd/hub"),
            opts
        );
    }

    private void DestroySession()
    {
        if (_session != null)
        {
            _session.CloseApp();
            _session.Quit();

            _session.Dispose();
            _session = null;
        }
    }

    private string ToArgumentString(string[] arguments)
    {
        return arguments.Aggregate(
            "",
            (argumentString, argument) => $"{argumentString} \"{argument}\""
        );
    }

    public void RestartApp()
    {
        GetSession().CloseApp();
        GetSession().LaunchApp();
    }

    public void RestartApp(params string[] arguments)
    {
        if (arguments.Length == 0)
        {
            RestartApp();
        }
        else
        {
            CreateSession(arguments);
        }
    }

    protected WindowsDriver<WindowsElement> GetSession()
    {
        return _session ?? throw new Exception("Setup session first!");
    }

    [OneTimeTearDown]
    public virtual Task TeardownAsync()
    {
        DestroySession();

        return _environment!.TeardownAsync();
    }

    public void Sleep(int milliseconds)
    {
        Thread.Sleep(milliseconds);
    }

    public void WaitFor(Action check, int timeout = 2_000, int numberOfChecks = 5)
    {
        int sleepDuration = timeout / numberOfChecks;
        Exception? previousException = null;

        while (numberOfChecks > 0)
        {
            numberOfChecks--;

            try
            {
                check();
                return;
            }
            catch (Exception e)
            {
                previousException = e;

                Thread.Sleep(sleepDuration);
            }
        }

        throw new Exception("WaitFor: check failed!", previousException);
    }

    public bool IsAppRunning()
    {
        using var proc = GetAppProcess();

        return proc is { HasExited: false };
    }

    public Process? GetAppProcess()
    {
        return Process
            .GetProcesses()
            .FirstOrDefault(
                (p) => p.MainWindowHandle.ToString() == GetSession().CurrentWindowHandle
            );
    }

    public byte[] GetFixtureFromFile(string filename)
    {
        return File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, filename));
    }
}
