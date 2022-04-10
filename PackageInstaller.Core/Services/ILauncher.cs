namespace PackageInstaller.Core.Services;

public interface ILauncher
{
    public Task LaunchAsync(Uri uri);

    public Task LaunchFolderAsync(string folderPath);
}
