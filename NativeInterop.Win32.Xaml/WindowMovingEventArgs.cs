using System;

namespace NativeInterop.Win32.Xaml
{
    public class WindowMovingEventArgs : EventArgs
    {
        public DesktopWindow Window { get; private set; }
        public WindowPosition NewPosition { get; private set; }
        public int Top { get; private set; }
        public int Left { get; private set; }
        public WindowMovingEventArgs(DesktopWindow window, WindowPosition windowPosition)
        {
            Window = window;
            NewPosition = new(windowPosition.Top, windowPosition.Left);
        }
    }
}
