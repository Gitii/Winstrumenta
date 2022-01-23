using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using PackageInstaller.Core.Services;
using PackageInstaller.Core.Helpers;

namespace PackageInstaller.IconThemes;

class IconThemeManager : IIconThemeManager
{
    private List<IconTheme> _themes;
    private IReadOnlyList<IIconTheme> _themesRo;

    public IconThemeManager()
    {
        _themes = new List<IconTheme>();
        _themesRo = new ReadOnlyCollection<IIconTheme>(
            _themes.WrapAs<IconTheme, IIconTheme>(theme => theme)
        );
        ActiveIconTheme = new NullIconTheme();
    }

    public IReadOnlyList<IIconTheme> AvailableThemes => _themesRo;
    public IIconTheme ActiveIconTheme { get; set; }

    public async Task LoadThemesAsync()
    {
        Package package = Package.Current;
        var optionalThemes = package.Dependencies.Where((d) => d.IsOptional).ToList();

        foreach (var theme in optionalThemes)
        {
            var rootFolder = theme.InstalledLocation;
            var assetFolder = await rootFolder.GetFolderAsync("Assets");
            var mappingFile = await assetFolder.GetFileAsync("mapping.csv");
            var licenseFile = await rootFolder.GetFileAsync("LICENSE");

            var mappingFileStream = await mappingFile
                .OpenStreamForReadAsync()
                .ConfigureAwait(false);
            await using var _ = mappingFileStream.ConfigureAwait(false);
            var licenseFileStream = await licenseFile
                .OpenStreamForReadAsync()
                .ConfigureAwait(false);
            await using var __ = licenseFileStream.ConfigureAwait(false);

            var iconsFactory = async () =>
            {
                var iconFile = await assetFolder.GetFileAsync("icons.zip");
                return await iconFile.OpenStreamForReadAsync().ConfigureAwait(false);
            };

            var iconTheme = new IconTheme(
                theme.DisplayName,
                theme.Description ?? String.Empty,
                iconsFactory
            );
            await iconTheme
                .LoadFromStreamAsync(mappingFileStream, licenseFileStream)
                .ConfigureAwait(false);

            _themes.Add(iconTheme);
        }

        ActiveIconTheme = AvailableThemes.FirstOrDefault() ?? new NullIconTheme();
    }
}
