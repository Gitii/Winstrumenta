using Windows.UI;

namespace NativeInterop.Win32;

public interface INativeWindowListener
{
    void OnActivated(NativeWindow nativeWindow);
    void OnClosing(NativeWindow nativeWindow);
    void OnMoving(NativeWindow nativeWindow);
    void OnSizing(NativeWindow nativeWindow);

    void OnDpiChanged(NativeWindow nativeWindow, uint dpi);

    void OnSystemThemeChanged(NativeWindow nativeWindow, Color foreground, Color background);
}
