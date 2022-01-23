#region

using Windows.UI;

#endregion

namespace NativeInterop.Win32;

public interface INativeWindowListener
{
    void OnActivated(NativeWindow nativeWindow);
    void Closing(NativeWindow nativeWindow);
    void Moving(NativeWindow nativeWindow);
    void Sizing(NativeWindow nativeWindow);
    void DpiChanged(NativeWindow nativeWindow, uint dpi);

    void OnSystemThemeChanged(NativeWindow nativeWindow, Color foreground, Color Background);
}
