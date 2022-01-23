namespace PackageInstaller.Core.Services;

public interface IWsl
{
    Task<WslDistribution[]> GetAllInstalledDistributionsAsync();
}
