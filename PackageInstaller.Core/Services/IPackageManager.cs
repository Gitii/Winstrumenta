namespace PackageInstaller.Core.Services
{
    public interface IPackageManager
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

        public struct PackageStatus
        {
            public PackageInstallationStatus Status { get; init; }
            public string? InstalledPackageVersion { get; set; }
        }

        public Task<PackageStatus> CheckInstallationStatus(
            WslDistribution distribution,
            string packageName,
            string packageVersion
        );

        public Task InstallPackage(string filePath) { }
    }
}
