using System;
using FluentAssertions;
using NUnit.Framework;

namespace PackageInstaller.Core.Services.Tests
{
    /// <summary>
    /// Based on https://salsa.debian.org/python-debian-team/python-debian/-/blob/master/lib/debian/tests/test_debian_support.py
    /// </summary>
    public class VersionTests
    {
        public void _test_version(string full_version, string epoch, string upstream, string debian)
        {
            var v = new NativeVersion(full_version);

            v.FullVersion.Should().BeEquivalentTo(full_version, "full_version broken");
            v.Epoch.Should().BeEquivalentTo(epoch, "epoch broken");
            v.UpstreamVersion.Should().BeEquivalentTo(upstream, "upstream_version broken");
            v.DebianRevision.Should().BeEquivalentTo(debian, "debian_revision broken");
        }

        [Test]
        public void testversions()
        {
            _test_version("1:1.4.1-1", "1", "1.4.1", "1");
            _test_version("7.1.ds-1", "", "7.1.ds", "1");
            _test_version("10.11.1.3-2", "", "10.11.1.3", "2");
            _test_version("4.0.1.3.dfsg.1-2", "", "4.0.1.3.dfsg.1", "2");
            _test_version("0.4.23debian1", "", "0.4.23debian1", "");
            _test_version("1.2.10+cvs20060429-1", "", "1.2.10+cvs20060429", "1");
            _test_version("0.2.0-1+b1", "", "0.2.0", "1+b1");
            _test_version("4.3.90.1svn-r21976-1", "", "4.3.90.1svn-r21976", "1");
            _test_version("1.5+E-14", "", "1.5+E", "14");
            _test_version("20060611-0.0", "", "20060611", "0.0");
            _test_version("0.52.2-5.1", "", "0.52.2", "5.1");
            _test_version("7.0-035+1", "", "7.0", "035+1");
            _test_version("1.1.0+cvs20060620-1+2.6.15-8", "", "1.1.0+cvs20060620-1+2.6.15", "8");
            _test_version("1.1.0+cvs20060620-1+1.0", "", "1.1.0+cvs20060620", "1+1.0");
            _test_version("4.2.0a+stable-2sarge1", "", "4.2.0a+stable", "2sarge1");
            _test_version("1.8RC4b", "", "1.8RC4b", "");
            _test_version("0.9~rc1-1", "", "0.9~rc1", "1");
            _test_version("2:1.0.4+svn26-1ubuntu1", "2", "1.0.4+svn26", "1ubuntu1");
            _test_version("2:1.0.4~rc2-1", "2", "1.0.4~rc2", "1");

            var call = () => new NativeVersion("a1:1.8.8-070403-1~priv1");

            call.Should().Throw<ParseError>();
        }

        //[Test]
        //public void test_version_updating()
        //{
        //    var v = new NativeVersion("1:1.4.1-1");
        //    v.DebianRevision = "2";
        //    v.DebianRevision.Should().BeEquivalentTo("2");
        //    v.FullVersion.Should().BeEquivalentTo("1:1.4.1-2");
        //    v.UpstreamVersion = "1.4.2";
        //    v.UpstreamVersion.Should().BeEquivalentTo("1.4.2");
        //    v.FullVersion.Should().BeEquivalentTo("1:1.4.2-2");
        //    v.Epoch = "2";
        //    v.Epoch.Should().BeEquivalentTo("2");
        //    v.FullVersion.Should().BeEquivalentTo("2:1.4.2-2");
        //    v.ToString().Should().BeEquivalentTo(v.FullVersion);
        //    v.FullVersion = "1:1.4.1-1";
        //    v.FullVersion.Should().BeEquivalentTo("1:1.4.1-1");
        //    v.Epoch.Should().BeEquivalentTo("1");
        //    v.UpstreamVersion.Should().BeEquivalentTo("1.4.1");
        //    v.DebianRevision.Should().BeEquivalentTo("1");
        //}

        public static Func<T, T, bool> _get_truth_fn<T>(string cmp_oper) where T : IComparable<T>
        {
            if (cmp_oper == "<")
            {
                return (T a, T b) => a.CompareTo(b) < 0;
            }
            else if (cmp_oper == "<=")
            {
                return (a, b) => a.CompareTo(b) <= 0;
            }
            else if (cmp_oper == "==")
            {
                return (a, b) => a.CompareTo(b) == 0;
            }
            else if (cmp_oper == ">=")
            {
                return (a, b) => a.CompareTo(b) >= 0;
            }
            else if (cmp_oper == ">")
            {
                return (a, b) => a.CompareTo(b) > 0;
            }
            else
            {
                throw new ArgumentException(string.Format("invalid operator %s", cmp_oper));
            }
        }

        // Test comparison against all combinations of Version classes
        //
        //         This is does the real work for test_comparisons.
        //
        public void _test_comparison(string v1_str, string cmp_oper, string v2_str)
        {
            var a = new NativeVersion(v1_str);
            var b = new NativeVersion(v2_str);

            _get_truth_fn<NativeVersion>(cmp_oper)(a, b).Should().BeTrue();
        }

        [Test]
        // Test comparison against all combinations of Version classes
        public void test_comparisons()
        {
            _test_comparison("0", "<", "a");
            _test_comparison("1.0", "<", "1.1");
            _test_comparison("1.2", "<", "1.11");
            _test_comparison("1.0-0.1", "<", "1.1");
            _test_comparison("1.0-0.1", "<", "1.0-1");
            _test_comparison("1.0", "==", "1.0");
            _test_comparison("1.0-0.1", "==", "1.0-0.1");
            _test_comparison("1:1.0-0.1", "==", "1:1.0-0.1");
            _test_comparison("1:1.0", "==", "1:1.0");
            _test_comparison("1.0-0.1", "<", "1.0-1");
            _test_comparison("1.0final-5sarge1", ">", "1.0final-5");
            _test_comparison("1.0final-5", ">", "1.0a7-2");
            _test_comparison("0.9.2-5", "<", "0.9.2+cvs.1.0.dev.2004.07.28-1.5");
            _test_comparison("1:500", "<", "1:5000");
            _test_comparison("100:500", ">", "11:5000");
            _test_comparison("1.0.4-2", ">", "1.0pre7-2");
            _test_comparison("1.5~rc1", "<", "1.5");
            _test_comparison("1.5~rc1", "<", "1.5+b1");
            _test_comparison("1.5~rc1", "<", "1.5~rc2");
            _test_comparison("1.5~rc1", ">", "1.5~dev0");
        }
    }
}