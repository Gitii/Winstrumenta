using System.IO;
using System.Threading.Tasks;
using PackageInstaller.Core.Services;

namespace PackageInstaller.IconThemes;

class NullIconTheme : IIconTheme
{
    public string Name { get; } = "No Icons";
    public string Description { get; } = "A fake icon theme that doesn't return any icons.";
    public string License { get; } = "MIT";

    public Task<Stream?> GetSvgIconByName(string packageThemeName)
    {
        return Task.FromResult<Stream?>(null);
    }
}
