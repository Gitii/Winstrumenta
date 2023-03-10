using System.Diagnostics;
using FluentAssertions;

namespace Winstrumenta.Package.Tests;

public class GettingStartedPageTests : TestSuiteBase
{
    [Test]
    public void ShouldShowGettingStartedPageWhenNoArgumentsProvided()
    {
        var session = GetSession();
        session.FindElementByAccessibilityId("Title").Text.Should().Be("Getting started!");
        session.FindElementByAccessibilityId("Close").Click();

        WaitFor(() => IsAppRunning().Should().BeFalse("App should have been closed by clicking the close button"));
    }

    [Test]
    public void ShouldOpenExplorerWhenClickedOnLinkInPickSection()
    {
        GetSession().FindElementByAccessibilityId("PickSection").Click();

        Sleep(2000);

        HasExplorerAsChildProcess().Should().BeTrue();
    }

    [Test]
    public void ShouldOpenExplorerWhenClickedOnLink()
    {
        GetSession().FindElementByAccessibilityId("PickHyperLink").Click();

        Sleep(2000);

        HasExplorerAsChildProcess().Should().BeTrue();
    }

    private bool HasExplorerAsChildProcess()
    {
        var process = GetAppProcess();

        if (process == null)
        {
            return false;
        }

        return process
            .GetChildProcesses()
            .Any(childProcess => childProcess.ProcessName == "explorer.exe");
    }

}
