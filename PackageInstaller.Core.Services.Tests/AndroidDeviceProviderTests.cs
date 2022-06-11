using System;
using System.Threading.Tasks;
using Community.Wsa.Sdk;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace PackageInstaller.Core.Services.Tests;

public class AndroidDeviceProviderTests
{
    [Test]
    public async Task GetAllInstalledDistributionsAsync_ShouldReturnEmptyListAsync()
    {
        var adb = A.Fake<IAdb>();
        A.CallTo(() => adb.IsInstalled).Returns(false);

        var adp = new AndroidDeviceProvider(adb);

        var list = await adp.GetAllInstalledDistributionsAsync("");

        list.Alerts.Should().BeEmpty();
        list.InstalledDistributions.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllInstalledDistributionsAsync_ShouldReturnAdbWarningAsync()
    {
        var adb = A.Fake<IAdb>();
        A.CallTo(() => adb.IsInstalled).Returns(false);

        var adp = new AndroidDeviceProvider(adb);

        var list = await adp.GetAllInstalledDistributionsAsync(AndroidDeviceProvider.APK_FILE_EXTENSION);

        list.InstalledDistributions.Should().BeEmpty();
        list.Alerts.Should().HaveCount(1);
    }

    [Test]
    public async Task GetAllInstalledDistributionsAsync_ShouldReturnNoDevicesFoundWarningAsync()
    {
        var adb = A.Fake<IAdb>();
        A.CallTo(() => adb.IsInstalled).Returns(true);
        A.CallTo(() => adb.ListDevicesAsync()).Returns(Task.FromResult(Array.Empty<KnownDevice>()));

        var adp = new AndroidDeviceProvider(adb);

        var list = await adp.GetAllInstalledDistributionsAsync(AndroidDeviceProvider.APK_FILE_EXTENSION);

        list.InstalledDistributions.Should().BeEmpty();
        list.Alerts.Should().HaveCount(1);
    }
}
