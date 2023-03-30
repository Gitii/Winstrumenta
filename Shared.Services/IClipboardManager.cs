namespace Shared.Services;

public interface IClipboardManager
{
    public Task CopyAsync(string content);
}
