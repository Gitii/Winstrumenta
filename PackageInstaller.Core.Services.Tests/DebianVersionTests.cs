using System;
using FluentAssertions;
using NUnit.Framework;

namespace PackageInstaller.Core.Services.Tests;

public class DebianVersionTests
{
    private static readonly object?[] VersionParseTestCases =
    {
        new object?[] { "1:1.4.1-1", 1u, "1.4.1", "1" }, new object?[] { "7.1.ds-1", 0u, "7.1.ds", "1" },
        new object?[] { "10.11.1.3-2", 0u, "10.11.1.3", "2" },
        new object?[] { "4.0.1.3.dfsg.1-2", 0u, "4.0.1.3.dfsg.1", "2" },
        new object?[] { "0.4.23debian1", 0u, "0.4.23debian1", null },
        new object?[] { "1.2.10+cvs20060429-1", 0u, "1.2.10+cvs20060429", "1" },
        new object?[] { "0.2.0-1+b1", 0u, "0.2.0", "1+b1" },
        new object?[] { "4.3.90.1svn-r21976-1", 0u, "4.3.90.1svn-r21976", "1" },
        new object?[] { "1.5+E-14", 0u, "1.5+E", "14" }, new object?[] { "20060611-0.0", 0u, "20060611", "0.0" },
        new object?[] { "0.52.2-5.1", 0u, "0.52.2", "5.1" }, new object?[] { "7.0-035+1", 0u, "7.0", "035+1" },
        new object?[] { "1.1.0+cvs20060620-1+2.6.15-8", 0u, "1.1.0+cvs20060620-1+2.6.15", "8" },
        new object?[] { "1.1.0+cvs20060620-1+1.0", 0u, "1.1.0+cvs20060620", "1+1.0" },
        new object?[] { "4.2.0a+stable-2sarge1", 0u, "4.2.0a+stable", "2sarge1" },
        new object?[] { "1.8RC4b", 0u, "1.8RC4b", null }, new object?[] { "0.9~rc1-1", 0u, "0.9~rc1", "1" },
        new object?[] { "2:1.0.4+svn26-1ubuntu1", 2u, "1.0.4+svn26", "1ubuntu1" },
        new object?[] { "2:1.0.4~rc2-1", 2u, "1.0.4~rc2", "1" },
    };

    [TestCaseSource(nameof(VersionParseTestCases))]
    public void ShouldParseVersion(string rawVersionString, uint epoch, string upstream, string? debian)
    {
        var v = new DebianVersion(rawVersionString);

        v.Epoch.Should().Be(epoch);
        v.UpstreamVersion.Should().BeEquivalentTo(upstream);
        v.DebianRevision.Should().BeEquivalentTo(debian);

        v.ToString().Should().BeEquivalentTo(rawVersionString);
    }

    [Test]
    public void ShouldNotParseInvalidVersion()
    {
        var call = () => new DebianVersion("a1:1.8.8-070403-1~priv1");

        call.Should().Throw<Exception>();
    }

    private static readonly object[] VersionComparisonTestSources =
    {
        new object?[] { "0", "<", "a" }, new object?[] { "1.0", "<", "1.1" }, new object?[] { "1.2", "<", "1.11" },
        new object?[] { "1.0-0.1", "<", "1.1" }, new object?[] { "1.0-0.1", "<", "1.0-1" },
        new object?[] { "1.0", "==", "1.0" }, new object?[] { "1.0-0.1", "==", "1.0-0.1" },
        new object?[] { "1:1.0-0.1", "==", "1:1.0-0.1" }, new object?[] { "1:1.0", "==", "1:1.0" },
        new object?[] { "1.0-0.1", "<", "1.0-1" }, new object?[] { "1.0final-5sarge1", ">", "1.0final-5" },
        new object?[] { "1.0final-5", ">", "1.0a7-2" },
        new object?[] { "0.9.2-5", "<", "0.9.2+cvs.1.0.dev.2004.07.28-1.5" },
        new object?[] { "1:500", "<", "1:5000" }, new object?[] { "100:500", ">", "11:5000" },
        new object?[] { "1.0.4-2", ">", "1.0pre7-2" }, new object?[] { "1.5~rc1", "<", "1.5" },
        new object?[] { "1.5~rc1", "<", "1.5+b1" }, new object?[] { "1.5~rc1", "<", "1.5~rc2" },
        new object?[] { "1.5~rc1", ">", "1.5~dev0" },
    };

    [TestCaseSource(nameof(VersionComparisonTestSources))]
    public void ShouldCompareVersion(string leftRawVersion, string comparisonOperator, string rightRawVersion)
    {
        var left = new DebianVersion(leftRawVersion);
        var right = new DebianVersion(rightRawVersion);

        GetVersionComparator<DebianVersion>(comparisonOperator)(left, right).Should()
            .BeTrue($"{leftRawVersion} {comparisonOperator} {right}");
    }

    public static Func<T, T, bool> GetVersionComparator<T>(string comparisonOperator)
        where T : IComparable<T>
    {
        switch (comparisonOperator)
        {
            case "<":
                return (T a, T b) => a.CompareTo(b) < 0;
            case "<=":
                return (a, b) => a.CompareTo(b) <= 0;
            case "==":
                return (a, b) => a.CompareTo(b) == 0;
            case ">=":
                return (a, b) => a.CompareTo(b) >= 0;
            case ">":
                return (a, b) => a.CompareTo(b) > 0;
            default:
                throw new ArgumentException(
                    $"invalid operator {comparisonOperator}",
                    nameof(comparisonOperator)
                );
        }
    }
}
