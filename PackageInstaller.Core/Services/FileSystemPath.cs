namespace PackageInstaller.Core.Services
{
    public record class FileSystemPath(string UnixPath, string WindowsPath)
    {
        public string UnixPath { get; init; } =
            UnixPath ?? throw new ArgumentNullException(nameof(UnixPath));

        public string WindowsPath { get; init; } =
            WindowsPath ?? throw new ArgumentNullException(nameof(WindowsPath));
    }
}