using System;

namespace WindowExtensions;

public class WindowDpiChangedEventArgs : EventArgs
{
    public DesktopWindow Window { get; private set; }
    public int Dpi { get; private set; }

    public WindowDpiChangedEventArgs(DesktopWindow window, int newDpi)
    {
        Window = window;
        Dpi = newDpi;
    }
}
