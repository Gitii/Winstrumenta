namespace PackageInstaller.Core.Services;

public interface IMouseEventHooks
{
    event EventHandler<MouseEventExtArgs> MouseUpExt;

    event EventHandler<MouseEventExtArgs> MouseMoveExt;
}
