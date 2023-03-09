using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace Shared.Services.Implementations.WinUI;

public class ClipboardManagerImpl : IClipboardManager
{
    public Task CopyAsync(string content)
    {
        var package = new DataPackage();
        package.SetText(content);

        Clipboard.SetContent(package);
        return Task.CompletedTask;
    }
}
