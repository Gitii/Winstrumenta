using FluentAssertions;
using NUnit.Framework;
using PackageInstaller.Core.Services.Tests.Fixtures;

namespace PackageInstaller.Core.Services.Tests;

public class ControlFileTests
{
    [Test]
    public void Test_ParseEmpty()
    {
        ControlFile cf = new ControlFile();
        cf.Entries.Should().BeEmpty();
        cf.Parse("");
        cf.Entries.Should().BeEmpty();
    }

    [Test]
    public void Test_ParseSingleEntry()
    {
        ControlFile cf = new ControlFile();
        cf.Parse(ControlFileFixtures.SingleEntryRaw);
        cf.Entries.Should().Equal(ControlFileFixtures.SingleEntry);
    }

    [Test]
    public void Test_ParseSingleMultilineEntry()
    {
        ControlFile cf = new ControlFile();
        cf.Parse(ControlFileFixtures.SingleMultilineEntryRaw);
        cf.Entries.Should().Equal(ControlFileFixtures.SingleMultilineEntry);
    }

    [Test]
    public void Test_ParseSingleMultilineEntry2()
    {
        ControlFile cf = new ControlFile();
        cf.Parse(ControlFileFixtures.SingleMultilineEntry2Raw);
        cf.Entries
            .Should()
            .Equal(ControlFileFixtures.SingleMultiline2Entry);
    }
}
