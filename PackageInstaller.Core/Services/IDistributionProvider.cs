namespace PackageInstaller.Core.Services;

public interface IDistributionProvider
{
    Task<Distribution[]> GetAllInstalledDistributionsAsync();
}
