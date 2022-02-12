using WindowsHook;

namespace PackageInstaller.Core.Services;

public class GlobalHook : IGlobalHook
{
    private IKeyboardMouseEvents _globalHook;
    private MouseEventHooks _wrappedHooks;

    public GlobalHook()
    {
        _globalHook = Hook.GlobalEvents();
        _wrappedHooks = new MouseEventHooks(_globalHook);
    }

    public IMouseEventHooks Hooks => _wrappedHooks;

    private void ReleaseUnmanagedResources()
    {
        _globalHook.Dispose();
        _wrappedHooks.Dispose();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~GlobalHook()
    {
        ReleaseUnmanagedResources();
    }
}
