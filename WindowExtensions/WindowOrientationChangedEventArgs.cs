using System;

namespace WindowExtensions;

public class WindowOrientationChangedEventArgs : EventArgs
{
    public DesktopWindow Window { get; private set; }
    public DesktopWindow.Orientation Orientation { get; private set; }

    public WindowOrientationChangedEventArgs(
        DesktopWindow window,
        DesktopWindow.Orientation newOrientationi
    )
    {
        Window = window;
        Orientation = newOrientationi;
    }
}
