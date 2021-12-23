using System;

namespace WindowExtensions;

public class WindowSizingEventArgs : EventArgs
{
    public DesktopWindow Window { get; private set; }

    public WindowSizingEventArgs(DesktopWindow window)
    {
        Window = window;
    }
}
