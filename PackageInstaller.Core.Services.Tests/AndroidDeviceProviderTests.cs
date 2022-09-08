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

        var list = await adp.GetAllInstalledDistributionsAsync("").ConfigureAwait(false);

        list.Alerts.Should().BeEmpty();
        list.InstalledDistributions.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllInstalledDistributionsAsync_ShouldReturnAdbWarningAsync()
    {
        var adb = A.Fake<IAdb>();
        A.CallTo(() => adb.IsInstalled).Returns(false);

        var adp = new AndroidDeviceProvider(adb);

        var list = await adp.GetAllInstalledDistributionsAsync(
                AndroidDeviceProvider.APK_FILE_EXTENSION
            )
            .ConfigureAwait(false);

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

        var list = await adp.GetAllInstalledDistributionsAsync(
                AndroidDeviceProvider.APK_FILE_EXTENSION
            )
            .ConfigureAwait(false);

        list.InstalledDistributions.Should().BeEmpty();
        list.Alerts.Should().HaveCount(1);
    }

    private static object[] DEVICE_TEST_SOURCE = new object[]
    {
        new object[] { "", new Version(0, 0) },
        new object[] { "1", new Version(1, 0) },
        new object[] { "1.2", new Version(1, 2) },
        new object[] { "1.2.3", new Version(1, 2, 3) },
        new object[] { "1.2.3.4", new Version(1, 2, 3) },
    };

    [TestCaseSource(nameof(DEVICE_TEST_SOURCE))]
    public async Task GetAllInstalledDistributionsAsync_ShouldReturnDevicesAsync(
        string rawAndroidVersion,
        Version androidVersion
    )
    {
        var kd = new KnownDevice()
        {
            DeviceCode = "deviceCode",
            DeviceSerialNumber = "serialnumber",
            DeviceType = DeviceType.Device,
            ModelNumber = "modelnumber",
            ProductCode = "productcode",
            TransportId = "transid"
        };

        var wsaKd = new KnownDevice() { ModelNumber = IWsaApi.WSA_MODEL_NUMBER, };

        var deviceName = "dn";

        var adb = A.Fake<IAdb>();
        A.CallTo(() => adb.IsInstalled).Returns(true);
        A.CallTo(() => adb.ListDevicesAsync()).Returns(Task.FromResult(new[] { wsaKd, kd }));
        A.CallTo(() => adb.ExecuteCommandAsync(A<string>._, A<string[]>._))
            .Returns(deviceName)
            .Once()
            .Then.Returns(rawAndroidVersion)
            .Once();

        var adp = new AndroidDeviceProvider(adb);

        var list = await adp.GetAllInstalledDistributionsAsync(
                AndroidDeviceProvider.APK_FILE_EXTENSION
            )
            .ConfigureAwait(false);

        list.InstalledDistributions.Should().HaveCount(1);
        list.InstalledDistributions
            .Should()
            .Contain(
                new Distribution()
                {
                    Id = kd.DeviceSerialNumber,
                    IsAvailable = !kd.IsOffline,
                    IsRunning = !kd.IsOffline,
                    Origin = AndroidDeviceProvider.ORIGIN_ANDROID_DEVICE,
                    Type = DistributionType.Android,
                    Name = deviceName,
                    Version = androidVersion
                }
            );
        list.Alerts.Should().BeEmpty();
    }
}
