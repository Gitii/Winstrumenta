using System;

namespace NativeInterop.Win32.Xaml;

public class WindowSizingEventArgs : EventArgs
{
    public DesktopWindow Window { get; private set; }

    public WindowSizingEventArgs(DesktopWindow window)
    {
        Window = window;
    }
}
