using System;
using System.Threading.Tasks;
using Windows.Storage;
using System.Diagnostics;

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

    public Task LaunchWithAssociatedAppAsync(string filePath)
    {
        return LaunchWithAssociatedAppAsync(filePath, null);
    }

    public async Task LaunchWithAssociatedAppAsync(string filePath, string? appPath)
    {
        if (appPath != null)
        {
            Process.Start(appPath, filePath).Dispose();
        }
        else
        {
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            await Windows.System.Launcher.LaunchFileAsync(file).AsTask().ConfigureAwait(false);
        }
    }
}
