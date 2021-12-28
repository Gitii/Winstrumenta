namespace PackageInstaller.Core.Services
{
    public interface IDpkg
    {
        public Task<bool> IsPackageInstalled(string distroName, string packageName);
        public Task<PackageInfo> GetPackage(string distroName, string packageName);

        public readonly struct PackageInfo
        {
            public string Name { get; init; }
            public string Version { get; init; }
        }

        int CompareVersions(string versionA, string versionB);

        Task<(bool success, string[] logs)> Install(string distroName, string unixFilePath);
        Task<(bool success, string[] logs)> Uninstall(string distroName, string packageName);
        Task<(bool success, string[] logs)> Upgrade(string distroName, string unixFilePath);
        Task<(bool success, string[] logs)> Downgrade(string distroName, string unixFilePath);
    }
}
