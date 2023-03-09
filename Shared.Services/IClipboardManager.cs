using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services;

public interface IClipboardManager
{
    public Task CopyAsync(string content);
}
