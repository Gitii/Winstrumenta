using WindowsHook;

namespace PackageInstaller.Core.Services;

class MouseEventHooks : IMouseEventHooks, IDisposable
{
    private readonly IKeyboardMouseEvents _globalHook;

    public MouseEventHooks(IKeyboardMouseEvents hooks)
    {
        _globalHook = hooks;
    }

    public event EventHandler<MouseEventExtArgs>? MouseUpExt;
    public event EventHandler<MouseEventExtArgs>? MouseMoveExt;

    public void Dispose()
    {
        _globalHook.MouseUpExt -= OnMouseUpExt;
        _globalHook.MouseMoveExt -= OnMouseMoveExt;
    }

    private void OnMouseMoveExt(object? sender, WindowsHook.MouseEventExtArgs e)
    {
        MouseMoveExt?.Invoke(sender, new MouseEventExtArgs(e.X, e.Y));
    }

    private void OnMouseUpExt(object? sender, WindowsHook.MouseEventExtArgs e)
    {
        MouseUpExt?.Invoke(sender, new MouseEventExtArgs(e.X, e.Y));
    }
}
