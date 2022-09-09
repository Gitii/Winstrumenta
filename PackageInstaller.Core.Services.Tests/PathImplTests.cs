using System;
using FluentAssertions;
using NUnit.Framework;

namespace PackageInstaller.Core.Services.Tests;

public class PathImplTests
{
    [Test]
    [TestCase(null, "Path must not be empty or null")]
    [TestCase("", "Path must not be empty or null")]
    [TestCase(".", "The path is not rooted to a drive with a drive letter: .")]
    [TestCase(
        ".\\relative\\path",
        "The path is not rooted to a drive with a drive letter: .\\relative\\path"
    )]
    [TestCase("/", "The path is not rooted to a drive with a drive letter: /")]
    [TestCase("\\", "The path is not rooted to a drive with a drive letter: \\")]
    [TestCase("//awdawdawd", "The path is not rooted to a drive with a drive letter: //awdawdawd")]
    [TestCase("\\wsl$\\est", "Cannot convert a wsl share to a unix path")]
    [TestCase("\\wsl.localhost\\est", "Cannot convert a wsl share to a unix path")]
    public void ToUnixPath_ShouldThrowWhenPathIsInvalid(string? path, string exceptionMessage)
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
    public void ToUnixPath_ShouldConvertSimplePaths(string path, string convertedPath)
    {
        var pathImpl = new PathImpl();

        pathImpl.ToUnixPath(path).Should().BeEquivalentTo(convertedPath);
    }

    [Test]
    [TestCase("C:\\dir\\adawd/awdawd/////awdawd\\file", "/mnt/c/dir/adawd/awdawd/////awdawd/file")]
    [TestCase("C://adawd.awdawd", "/mnt/c/adawd.awdawd")]
    public void ToUnixPath_ShouldConvertLongPaths(string path, string convertedPath)
    {
        var pathImpl = new PathImpl();

        pathImpl.ToUnixPath(path).Should().BeEquivalentTo(convertedPath);
    }

    [Test]
    [TestCase("C:\\test.deb", "/mnt/c/test.deb", "C:\\test.deb")]
    [TestCase("/mnt/x/test.deb", "/mnt/x/test.deb", "x:\\test.deb")]
    public void ToFileSystemPath_ShouldConvertToFSPaths(
        string path,
        string unixPath,
        string windowsPath
    )
    {
        var pathImpl = new PathImpl();

        var p = pathImpl.ToFileSystemPath(path);
        p.UnixPath.Should().BeEquivalentTo(unixPath);
        p.WindowsPath.Should().BeEquivalentTo(windowsPath);
    }

    [Test]
    [TestCase("", "Invalid path ''")]
    [TestCase(" ", "Invalid path ' '")]
    [TestCase(".", "Invalid path '.'")]
    [TestCase("test.deb", "Invalid path 'test.deb'")]
    [TestCase("..//test.deb", "Invalid path '..//test.deb'")]
    public void ToFileSystemPath_ShouldThrowWhenInvalidPath(string path, string exceptionMessage)
    {
        var pathImpl = new PathImpl();

        var call = () => pathImpl.ToFileSystemPath(path);

        call.Should().Throw<Exception>().WithMessage(exceptionMessage);
    }

    [Test]
    [TestCase("\\\\wsl$\\test")]
    [TestCase("\\\\wsl.localhost\\test")]
    public void IsWslNetworkShare_ShouldReturnTrueWhenWslNetworkShare(string path)
    {
        var pathImpl = new PathImpl();

        pathImpl.IsWslNetworkShare(path).Should().BeTrue();
    }

    [Test]
    [TestCase("\\notAShare\\test")]
    [TestCase("\\\\share\\test")]
    public void IsWslNetworkShare_ShouldReturnFalseWhenNotNetworkShare(string path)
    {
        var pathImpl = new PathImpl();

        pathImpl.IsWslNetworkShare(path).Should().BeFalse();
    }

    [Test]
    [TestCase("\\\\share\\test")]
    public void IsNetworkShare_ShouldReturnTrueWhenNetworkShare(string path)
    {
        var pathImpl = new PathImpl();

        pathImpl.IsNetworkShare(path).Should().BeTrue();
    }

    [Test]
    [TestCase("C:\\notAShare")]
    public void IsNetworkShare_ShouldReturnFalseWhenNotWslNetworkShare(string path)
    {
        var pathImpl = new PathImpl();

        pathImpl.IsNetworkShare(path).Should().BeFalse();
    }
}
