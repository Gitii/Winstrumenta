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
        public string Version { get; init; }
    }

    public readonly struct PackageMetaData
    {
        public string Package { get; init; }
        public string Version { get; init; }
        public string Architecture { get; init; }
        public string Description { get; init; }

        public IReadOnlyDictionary<string, string> AllFields { get; init; }
    }

    /// <summary>
    /// Checks if the specified package is installed in the specified distro.
    /// </summary>
    /// <param name="distroName">The name of the distribution.</param>
    /// <param name="packageName">The name of the package.</param>
    /// <returns></returns>
    public Task<bool> IsPackageInstalled(string distroName, string packageName);

    public Task<PackageInfo> GetInstalledPackageInfo(string distroName, string packageName);

    public Task<PackageMetaData> ExtractPackageMetaData(FileSystemPath filePath);

    public Task<(bool isSupported, string? reason)> IsPackageSupported(FileSystemPath filePath);

    /// <summary>
    /// Compares two versions and returns the status (same version, newer or older version).
    /// </summary>
    /// <param name="baseVersion">The reference version. Think of it like the "installed package".</param>
    /// <param name="otherVersion">The other version. Think of it as the "to be installed version".</param>
    /// <returns></returns>
    PackageInstallationStatus CompareVersions(string baseVersion, string otherVersion);

    Task<(bool success, string logs)> Install(string distroName, FileSystemPath filePath);
    Task<(bool success, string logs)> Uninstall(string distroName, string packageName);
    Task<(bool success, string logs)> Upgrade(string distroName, FileSystemPath filePath);
    Task<(bool success, string logs)> Downgrade(string distroName, FileSystemPath filePath);

    Task<bool> IsSupportedByDistribution(string distroName);
}

public static class PlatformDependentPackageManagerExtensions
{
    public static async Task<IPlatformDependentPackageManager> GetSupportedManager(
        this IEnumerable<IPlatformDependentPackageManager> managers,
        FileSystemPath packageFilePath
    )
    {
        IList<string> rejectionReasons = new List<string>();

        foreach (var manager in managers)
        {
            (var isSupported, string? reason) = await manager.IsPackageSupported(packageFilePath);

            if (isSupported)
            {
                return manager;
            }
            else
            {
                rejectionReasons.Add($"{manager.GetType().Name}: {reason ?? "Unknown reason"}");
            }
        }

        var message =
            $"No package manager supported this package:{Environment.NewLine}{String.Join(Environment.NewLine, rejectionReasons)}";
        throw new Exception(message);
    }

    public static async Task<IPlatformDependentPackageManager> GetSupportedManager(
        this IEnumerable<IPlatformDependentPackageManager> managers,
        FileSystemPath packageFilePath,
        string distroName
    )
    {
        IList<string> rejectionReasons = new List<string>();

        foreach (var manager in managers)
        {
            if (!await manager.IsSupportedByDistribution(distroName))
            {
                continue;
            }

            (var isSupported, string? reason) = await manager.IsPackageSupported(packageFilePath);

            if (isSupported)
            {
                return manager;
            }
            else
            {
                rejectionReasons.Add($"{manager.GetType().Name}: {reason ?? "Unknown reason"}");
            }
        }

        var message =
            $"No package manager supported this package:{Environment.NewLine}{String.Join(Environment.NewLine, rejectionReasons)}";
        throw new Exception(message);
    }
}