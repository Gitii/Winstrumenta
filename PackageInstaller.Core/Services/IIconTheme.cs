namespace PackageInstaller.Core.Services;

public interface IIconTheme
{
    public string Name { get; }

    public string Description { get; }

    public string License { get; }

    public Task<Stream?> GetSvgIconByNameAsync(string packageThemeName);
}
