namespace NativeInterop.Win32;

enum ZOrder : int
{
    HWND_BOTTOM = 1,
    HWND_NOTOPMOST = -2,
    HWND_TOP = 0,
    HWND_TOPMOST = -1,
}
