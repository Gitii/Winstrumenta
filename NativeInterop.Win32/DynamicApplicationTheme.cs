namespace NativeInterop.Win32;

/// <summary>
/// Declares the theme preference for an app.
/// </summary>
public enum DynamicApplicationTheme
{
    /// <summary>Use the **Light** default theme.</summary>
    Light,

    /// <summary>Use the **Dark** default theme.</summary>
    Dark,

    /// <summary>Use the currently active default theme (based on the system theme).</summary>
    Auto,
}
