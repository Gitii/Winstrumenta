namespace NativeInterop.Win32;

public enum StartupLocation
{
    /// <summary>
    /// Let the operation system handle the initial location of the window.
    /// </summary>
    Default,

    /// <summary>
    /// Center the window on the screen when it's first shown.
    /// </summary>
    CenterScreen,

    /// <summary>
    /// Center the window based on the main window of the parent process (the process which started this application) when it's first shown.
    /// </summary>
    CenterParentProcess
}
