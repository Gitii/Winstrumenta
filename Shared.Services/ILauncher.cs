namespace Shared.Services;

public interface ILauncher
{
    public Task LaunchAsync(Uri uri);

    public Task LaunchFolderAsync(string folderPath);

    public Task LaunchWithAssociatedAppAsync(string filePath);
    public Task LaunchWithAssociatedAppAsync(string filePath, string appPath);
}
