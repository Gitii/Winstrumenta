using System;

namespace WindowExtensions;

public class WindowKeyDownEventArgs : EventArgs
{
    public DesktopWindow Window { get; private set; }
    public int Key { get; private set; }

    public WindowKeyDownEventArgs(DesktopWindow window, int key)
    {
        Window = window;
        Key = key;
    }
}
