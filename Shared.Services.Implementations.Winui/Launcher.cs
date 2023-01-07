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
}
