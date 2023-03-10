namespace Shared.Services;

public interface ILauncher
{
    public Task LaunchAsync(Uri uri);

    public Task LaunchFolderAsync(string folderPath);
    Task LaunchDefaultAppsSettingsPageAsync();
    Task<bool> VerifyThatAllFileTypeAssociationsAreRegisteredAsync();
}
