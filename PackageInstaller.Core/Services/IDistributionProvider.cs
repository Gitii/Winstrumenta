namespace PackageInstaller.Core.Services;

public interface IDistributionProvider
{
    /// <summary>
    /// Returns a <see cref="DistributionList"/> which contains all installed (supported) distributions and (if available) a list of alerts for the end user.
    /// </summary>
    /// <param name="packageExtensionHint">Extension (without leading dot) of the package that is being managed.</param>
    /// <returns></returns>
    Task<DistributionList> GetAllInstalledDistributionsAsync(string packageExtensionHint);
}
