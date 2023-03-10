using System;
using System.Threading.Tasks;

namespace Shared.Services.Implementations.WinUI;

public class Launcher : ILauncher
{
    public Task LaunchAsync(Uri uri)
    {
        return Windows.System.Launcher.LaunchUriAsync(uri).AsTask();
    }

    public Task LaunchFolderAsync(string folderPath)
    {
        return Windows.System.Launcher.LaunchFolderPathAsync(folderPath).AsTask();
    }

    public Task LaunchDefaultAppsSettingsPageAsync()
    {
        return Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:defaultapps")).AsTask();
    }

    public async Task<bool> VerifyThatAllFileTypeAssociationsAreRegisteredAsync()
    {
        return false; // not possible to actually check
    }
}
