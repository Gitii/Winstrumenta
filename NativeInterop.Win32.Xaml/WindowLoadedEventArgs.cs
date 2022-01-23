using System;

namespace NativeInterop.Win32.Xaml;

public class WindowLoadedEventArgs : EventArgs
{
    public DesktopWindow Window { get; private set; }

    public WindowLoadedEventArgs(DesktopWindow window)
    {
        Window = window;
    }
}
