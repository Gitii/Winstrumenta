using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using ICSharpCode.SharpZipLib.Tar;
using PackageInstaller.Core.Services;
using PackageInstaller.Core.Helpers;

namespace PackageInstaller.IconThemes
{
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
        }

        public IReadOnlyList<IIconTheme> AvailableThemes => _themesRo;
        public IIconTheme ActiveIconTheme { get; set; }

        public async Task LoadThemes()
        {
            Package package = Package.Current;
            var optionalThemes = package.Dependencies.Where((d) => d.IsOptional).ToList();

            foreach (var theme in optionalThemes)
            {
                var rootFolder = theme.InstalledLocation;
                var assetFolder = await rootFolder.GetFolderAsync("Assets");
                var mappingFile = await assetFolder.GetFileAsync("mapping.csv");
                var licenseFile = await rootFolder.GetFileAsync("LICENSE");

                await using var mappingFileStream = await mappingFile.OpenStreamForReadAsync();
                await using var licenseFileStream = await licenseFile.OpenStreamForReadAsync();

                var iconsFactory = async () =>
                {
                    var iconFile = await assetFolder.GetFileAsync("icons.zip");
                    return await iconFile.OpenStreamForReadAsync();
                };

                var iconTheme = new IconTheme(theme.DisplayName, theme.Description ?? String.Empty, iconsFactory);
                await iconTheme.LoadFromStream(
                    mappingFileStream,
                    licenseFileStream);

                _themes.Add(iconTheme);
            }

            ActiveIconTheme = AvailableThemes.FirstOrDefault() ?? new NullIconTheme();
        }
    }
}