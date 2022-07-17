using System;
using System.Threading.Tasks;
using Community.Wsl.Sdk;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace PackageInstaller.Core.Services.Tests;

public class WslProviderTests
{
    [TestCase(WsaProvider.ANDROID_PACKAGE_EXTENSION, false, false)]
    [TestCase(WsaProvider.ANDROID_PACKAGE_EXTENSION, true, false)]
    public async Task GetAllInstalledDistributionsAsync_ShouldReturnWarningsAsync(
        string extension,
        bool isWsaSupported,
        bool isWsaInstalled
    )
    {
        var wslApi = A.Fake<IWslApi>();
        var msg = "msg";
        A.CallTo(() => wslApi.IsWslSupported(out msg)).Returns(isWsaSupported);
        A.CallTo(() => wslApi.IsInstalled).Returns(isWsaInstalled);

        var wsl = new WslProvider(wslApi);

        var list = await wsl.GetAllInstalledDistributionsAsync(extension).ConfigureAwait(false);

        list.Alerts.Should().NotBeEmpty();
        list.InstalledDistributions.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllInstalledDistributionsAsync_ShouldReturnWslDeviceAsync()
    {
        var devices = new DistroInfo[]
        {
            new DistroInfo()
            {
                DistroName = "docker-desktop-data",
                IsDefault = false,
                WslVersion = 2,
            },
            new DistroInfo()
            {
                DistroName = "docker-desktop",
                IsDefault = false,
                WslVersion = 2,
            },
            new DistroInfo()
            {
                DistroName = "Ubuntu 40.2",
                IsDefault = false,
                WslVersion = 2,
            },
        };

        var wslApi = A.Fake<IWslApi>();
        var msg = "msg";
        A.CallTo(() => wslApi.IsWslSupported(out msg)).Returns(true);
        A.CallTo(() => wslApi.IsInstalled).Returns(true);
        A.CallTo(() => wslApi.GetDistributionList()).Returns(devices);

        var wsl = new WslProvider(wslApi);

        var list = await wsl.GetAllInstalledDistributionsAsync("").ConfigureAwait(false);

        list.Alerts.Should().BeEmpty();
        list.InstalledDistributions.Should().HaveCount(1);
    }

    private static readonly object[] DEVICES_TEST_SOURCES = new[] { Array.Empty<DistroInfo>(), };

    [TestCaseSource(nameof(DEVICES_TEST_SOURCES))]
    public async Task GetAllInstalledDistributionsAsync_ShouldReturnNoDistrosInstalledAsync(
        DistroInfo[] distroList
    )
    {
        var wslApi = A.Fake<IWslApi>();
        var msg = "msg";
        A.CallTo(() => wslApi.IsWslSupported(out msg)).Returns(true);
        A.CallTo(() => wslApi.IsInstalled).Returns(true);
        A.CallTo(() => wslApi.GetDistributionList()).Returns(distroList);

        var wsl = new WslProvider(wslApi);

        var list = await wsl.GetAllInstalledDistributionsAsync("").ConfigureAwait(false);

        list.Alerts.Should().HaveCount(1);
        list.InstalledDistributions.Should().BeEmpty();
    }
}
