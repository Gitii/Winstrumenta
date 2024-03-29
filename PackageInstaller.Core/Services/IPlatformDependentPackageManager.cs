﻿using Shared.Services;

namespace PackageInstaller.Core.Services;

public interface IPlatformDependentPackageManager
{
    public enum PackageInstallationStatus
    {
        /// <summary>
        /// The package isn't installed.
        /// </summary>
        NotInstalled,

        /// <summary>
        /// The package with the same version is installed.
        /// </summary>
        InstalledSameVersion,

        /// <summary>
        /// An older package is installed.
        /// </summary>
        InstalledOlderVersion,

        /// <summary>
        /// A newer version is installed.
        /// </summary>
        InstalledNewerVersion
    }

    public readonly struct PackageInfo
    {
        public string Name { get; init; }
        public string VersionCode { get; init; }
    }

    public readonly struct PackageMetaData
    {
        public string PackageName { get; init; }
        public string PackageLabel { get; init; }
        public string VersionCode { get; init; }

        public string VersionLabel { get; init; }

        public string Architecture { get; init; }
        public string Description { get; init; }

        /// <summary>
        /// Name of the image (any format)
        /// </summary>
        public string? IconName { get; init; }

        /// <summary>
        /// Image (encoded as png image)
        /// </summary>
        public byte[]? IconData { get; init; }

        public IReadOnlyDictionary<string, string> AllFields { get; init; }
    }

    /// <summary>
    /// Checks if the specified package is installed in the specified distro.
    /// </summary>
    /// <param name="deviceId">The name of the distribution.</param>
    /// <param name="packageName">The name of the package.</param>
    /// <returns></returns>
    public Task<bool> IsPackageInstalledAsync(string deviceId, string packageName);

    public Task<PackageInfo> GetInstalledPackageInfoAsync(string deviceId, string packageName);

    public Task<PackageMetaData> ExtractPackageMetaDataAsync(FileSystemPath filePath);

    public Task<(bool isSupported, string? reason)> IsPackageSupportedAsync(
        FileSystemPath filePath
    );

    /// <summary>
    /// Compares two versions and returns the status (same version, newer or older version).
    /// </summary>
    /// <param name="baseVersion">The reference version. Think of it like the "installed package".</param>
    /// <param name="otherVersion">The other version. Think of it as the "to be installed version".</param>
    /// <returns></returns>
    PackageInstallationStatus CompareVersions(string baseVersion, string otherVersion);

    Task<(bool success, string logs)> InstallAsync(
        string distroName,
        FileSystemPath filePath,
        IProgressController progressController
    );

    Task<(bool success, string logs)> UninstallAsync(
        string deviceId,
        string packageName,
        IProgressController progressController
    );

    Task<(bool success, string logs)> UpgradeAsync(
        string deviceId,
        FileSystemPath filePath,
        IProgressController progressController
    );

    Task<(bool success, string logs)> DowngradeAsync(
        string deviceId,
        FileSystemPath filePath,
        IProgressController progressController
    );

    Task<bool> IsSupportedByDistributionAsync(string deviceId, string distroOrigin);

    Task LaunchAsync(string deviceId, string packageName, IProgressController progressController);
}
