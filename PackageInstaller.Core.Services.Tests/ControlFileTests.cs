using FluentAssertions;
using NUnit.Framework;

namespace PackageInstaller.Core.Services.Tests
{
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
            cf.Parse(ControlFileFixtures.ControlFileFixtures.SingleEntryRaw);
            cf.Entries.Should().Equal(ControlFileFixtures.ControlFileFixtures.SingleEntry);
        }

        [Test]
        public void Test_ParseSingleMultilineEntry()
        {
            ControlFile cf = new ControlFile();
            cf.Parse(ControlFileFixtures.ControlFileFixtures.SingleMultilineEntryRaw);
            cf.Entries.Should().Equal(ControlFileFixtures.ControlFileFixtures.SingleMultilineEntry);
        }

        [Test]
        public void Test_ParseSingleMultilineEntry2()
        {
            ControlFile cf = new ControlFile();
            cf.Parse(ControlFileFixtures.ControlFileFixtures.SingleMultilineEntry2Raw);
            cf.Entries
                .Should()
                .Equal(ControlFileFixtures.ControlFileFixtures.SingleMultiline2Entry);
        }
    }
}
