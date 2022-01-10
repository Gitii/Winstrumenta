using System;
using FluentAssertions;
using NUnit.Framework;

namespace PackageInstaller.Core.Services.Tests
{
    public class PathImplTests
    {
        [Test]
        [TestCase(null, "Only fully qualified paths are supported: null")]
        [TestCase("", "Only fully qualified paths are supported: ")]
        [TestCase(".", "Only fully qualified paths are supported: .")]
        [TestCase(
            ".\\relative\\path",
            "Only fully qualified paths are supported: .\\relative\\path"
        )]
        [TestCase("/", "Only fully qualified paths are supported: /")]
        [TestCase("\\", "Only fully qualified paths are supported: \\")]
        [TestCase(
            "//awdawdawd",
            "The path is not rooted to a drive with a drive letter: \\\\awdawdawd"
        )]
        public void Test_ToUnixPath_throws_invalid_paths(string? path, string exceptionMessage)
        {
            var pathImpl = new PathImpl();

            var call = () => pathImpl.ToUnixPath(path!);

            call.Should().Throw<Exception>().WithMessage(exceptionMessage);
        }

        [Test]
        [TestCase("C:\\", "/mnt/c/")]
        [TestCase("C://", "/mnt/c/")]
        [TestCase("C:\\a", "/mnt/c/a")]
        [TestCase("C://a", "/mnt/c/a")]
        public void Test_ToUnixPath_convert_simple_paths(string path, string convertedPath)
        {
            var pathImpl = new PathImpl();

            pathImpl.ToUnixPath(path).Should().BeEquivalentTo(convertedPath);
        }

        [Test]
        [TestCase(
            "C:\\dir\\adawd/awdawd/////awdawd\\file",
            "/mnt/c/dir/adawd/awdawd/////awdawd/file"
        )]
        [TestCase("C://adawd.awdawd", "/mnt/c/adawd.awdawd")]
        public void Test_ToUnixPath_convert_longer_paths(string path, string convertedPath)
        {
            var pathImpl = new PathImpl();

            pathImpl.ToUnixPath(path).Should().BeEquivalentTo(convertedPath);
        }
    }
}