using System.Runtime.InteropServices;

namespace NativeInterop.Win32;

[StructLayout(LayoutKind.Auto)]
public struct Size
{
    public Size()
    {
        Width = 0;
        Height = 0;
    }

    public Size(int width, int height) : this()
    {
        Width = width;
        Height = height;
    }

    public int Width { get; set; }

    public int Height { get; set; }
}
