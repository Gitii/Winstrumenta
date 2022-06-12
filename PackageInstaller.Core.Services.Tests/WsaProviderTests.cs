using System.Threading.Tasks;
using Community.Wsa.Sdk;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace PackageInstaller.Core.Services.Tests;

public class WsaProviderTests
{
    [Test]
    public async Task GetAllInstalledDistributionsAsync_ShouldReturnEmptyListAsync()
    {
        var wsaClient = A.Fake<IWsaApi>();
        var msg = "msg";
        A.CallTo(() => wsaClient.IsWsaSupported(out msg)).Returns(false);
        A.CallTo(() => wsaClient.IsWsaInstalled).Returns(false);

        var wsa = new WsaProvider(wsaClient);

        var list = await wsa.GetAllInstalledDistributionsAsync("");

        list.Alerts.Should().BeEmpty();
        list.InstalledDistributions.Should().BeEmpty();
    }

    [TestCase(WsaProvider.ANDROID_PACKAGE_EXTENSION, false, false)]
    [TestCase(WsaProvider.ANDROID_PACKAGE_EXTENSION, true, false)]
    public async Task GetAllInstalledDistributionsAsync_ShouldReturnWarningsAsync(
        string extension,
        bool isWsaSupported,
        bool isWsaInstalled
    )
    {
        var wsaClient = A.Fake<IWsaApi>();
        var msg = "msg";
        A.CallTo(() => wsaClient.IsWsaSupported(out msg)).Returns(isWsaSupported);
        A.CallTo(() => wsaClient.IsWsaInstalled).Returns(isWsaInstalled);

        var wsa = new WsaProvider(wsaClient);

        var list = await wsa.GetAllInstalledDistributionsAsync(extension);

        list.Alerts.Should().NotBeEmpty();
        list.InstalledDistributions.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllInstalledDistributionsAsync_ShouldReturnWsaDeviceAsync()
    {
        var wsaClient = A.Fake<IWsaApi>();
        var msg = "msg";
        A.CallTo(() => wsaClient.IsWsaSupported(out msg)).Returns(true);
        A.CallTo(() => wsaClient.IsWsaInstalled).Returns(true);

        var wsa = new WsaProvider(wsaClient);

        var list = await wsa.GetAllInstalledDistributionsAsync("");

        list.Alerts.Should().BeEmpty();
        list.InstalledDistributions.Should().HaveCount(1);
    }
}
