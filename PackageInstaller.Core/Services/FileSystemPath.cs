namespace PackageInstaller.Core.Services;

#pragma warning disable MA0015 // Specify the parameter name in ArgumentException
public record class FileSystemPath(string UnixPath, string WindowsPath)
{
    public string UnixPath { get; init; } =
        UnixPath ?? throw new ArgumentNullException(nameof(UnixPath));

    public string WindowsPath { get; init; } =
        WindowsPath ?? throw new ArgumentNullException(nameof(WindowsPath));
}
#pragma warning restore MA0015 // Specify the parameter name in ArgumentException
