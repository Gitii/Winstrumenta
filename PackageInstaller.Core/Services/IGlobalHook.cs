namespace PackageInstaller.Core.Services;

public interface IGlobalHook : IDisposable
{
    IMouseEventHooks Hooks { get; }
}
