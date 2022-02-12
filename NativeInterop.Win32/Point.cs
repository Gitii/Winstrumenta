using System.Runtime.InteropServices;

namespace NativeInterop.Win32;

[StructLayout(LayoutKind.Auto)]
public struct Point
{
    public Point()
    {
        X = 0;
        Y = 0;
    }

    public Point(int x, int y) : this()
    {
        X = x;
        Y = y;
    }

    public int X { get; set; }

    public int Y { get; set; }
}
