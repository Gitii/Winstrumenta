namespace PackageInstaller.Core.Services
{
    public readonly struct PackageMetaData
    {
        public string Package { get; init; }
        public string Version { get; init; }
        public string Architecture { get; init; }
        public string Description { get; init; }
        public PackageTypes PackageType { get; init; }

        public IReadOnlyDictionary<string, string> AllFields { get; init; }
    }

    public interface IPackageReader
    {
        public Task<PackageMetaData> ReadMetaData(string filePath);
        public Task<(bool isSupported, string? reason)> IsSupported(string filePath);
    }
}
