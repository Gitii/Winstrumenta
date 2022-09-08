using System.Diagnostics;
using FluentAssertions;

namespace Winstrumenta.Package.Tests;

public class GettingStartedPageTests : TestSuiteBase
{
    [Test]
    public void ShouldShowGettingStartedPageWhenNoArgumentsProvided()
    {
        GetSession().FindElementByAccessibilityId("Title").Text.Should().Be("Getting started!");
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
        var process = GetProcess();

        if (process == null)
        {
            return false;
        }

        return process
            .GetChildProcesses()
            .Any(childProcess => childProcess.ProcessName == "explorer.exe");
    }

    private Process? GetProcess()
    {
        return Process
            .GetProcesses()
            .FirstOrDefault(
                (p) => p.MainWindowHandle.ToString() == GetSession().CurrentWindowHandle
            );
    }
}
