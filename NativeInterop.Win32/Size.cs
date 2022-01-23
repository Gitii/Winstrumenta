namespace NativeInterop.Win32;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public struct Size
{
    public int Width { get; set; }

    public int Height { get; set; }
}
