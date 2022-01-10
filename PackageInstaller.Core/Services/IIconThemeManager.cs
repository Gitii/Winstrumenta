namespace PackageInstaller.Core.Services
{
    public interface IIconThemeManager
    {
        public IReadOnlyList<IIconTheme> AvailableThemes { get; }

        public IIconTheme ActiveIconTheme { get; set; }

        public Task LoadThemes();
    }
}