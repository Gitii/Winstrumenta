using System.IO;
using System.Threading.Tasks;

namespace PackageInstaller.Core.Services.WinUI;

public class NullIconTheme : IIconTheme
{
    public string Name { get; } = "No Icons";
    public string Description { get; } = "A fake icon theme that doesn't return any icons.";
    public string License { get; } = "MIT";

    public Task<Stream?> GetSvgIconByNameAsync(string packageThemeName)
    {
        return Task.FromResult<Stream?>(null);
    }
}
