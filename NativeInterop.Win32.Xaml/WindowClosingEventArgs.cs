using System;

namespace NativeInterop.Win32.Xaml
{
    public class WindowClosingEventArgs : EventArgs
    {
        public DesktopWindow Window { get; private set; }
        public WindowClosingEventArgs(DesktopWindow window)
        {
            Window = window;
        }

        public void TryCancel()
        {
            Window.IsClosing = true;
            Window.Close();
        }
    }
}
