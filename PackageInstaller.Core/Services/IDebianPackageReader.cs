namespace PackageInstaller.Core.Services;

public interface IDebianPackageReader
{
    public Task<DebianPackageMetaData> ReadMetaDataAsync(FileSystemPath filePath);
    public Task<(bool isSupported, string? reason)> IsSupportedAsync(FileSystemPath filePath);
}
