namespace PackageInstaller.Core.Services
{
    public class PackageReader : IPackageReader
    {
        private readonly IDebianPackageReader _debianPackageReader;

        public PackageReader(IDebianPackageReader debianPackageReader)
        {
            _debianPackageReader = debianPackageReader;
        }

        public async Task<PackageMetaData> ReadMetaData(string filePath)
        {
            var data = await Task.Run(async () => await _debianPackageReader.ReadMetaData(filePath));

            return new PackageMetaData()
            {
                Package = data.Package,
                Description = data.Description,
                Version = data.Version,
                Architecture = data.Architecture,
                PackageType = PackageTypes.Deb,
                AllFields = data.AllFields
            };
        }

        public Task<(bool isSupported, string? reason)> IsSupported(string filePath)
        {
            return _debianPackageReader.IsSupported(filePath);
        }
    }
}
