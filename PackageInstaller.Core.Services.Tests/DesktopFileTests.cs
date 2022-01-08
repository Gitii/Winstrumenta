using FluentAssertions;
using NUnit.Framework;
using PackageInstaller.Core.Services.Tests.ControlFileFixtures;

namespace PackageInstaller.Core.Services.Tests;

public class DesktopFileTests
{
    [Test]
    public void Test_ParseEmpty()
    {
        DesktopFile cf = new DesktopFile();
        cf.Groups.Should().BeEmpty();
        cf.Parse("");
        cf.Groups.Should().BeEmpty();
    }

    [Test]
    public void Test_ParseSingleGroupNoEntries()
    {
        DesktopFile cf = new DesktopFile();
        cf.Parse(DesktopFileFixtures.SingleGroupNoEntriesRaw);
        cf.Groups.Should().Equal(DesktopFileFixtures.SingleGroupNoEntries);
    }

    [Test]
    public void Test_ParseSingleGroupSingleEntry()
    {
        DesktopFile cf = new DesktopFile();
        cf.Parse(DesktopFileFixtures.SingleGroupSingleEntryRaw);
        cf.Groups.Should().Equal(DesktopFileFixtures.SingleGroupSingleEntry);
    }

    [Test]
    public void Test_ParseMultipleGroupsSingleEntry()
    {
        DesktopFile cf = new DesktopFile();
        cf.Parse(DesktopFileFixtures.MultipleGroupsSingleEntryRaw);
        cf.Groups.Should().Equal(DesktopFileFixtures.MultipleGroupsSingleEntry);
    }
}
