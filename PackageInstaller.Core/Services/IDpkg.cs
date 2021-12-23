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

        Task Install(string distroName, string unixfilePath, IProgress<(int, string)> progress);
    }
}
