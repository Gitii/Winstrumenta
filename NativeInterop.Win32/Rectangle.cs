namespace NativeInterop.Win32;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public struct Rectangle
{
    /// <summary>Specifies the <i>x</i>-coordinate of the upper-left corner of the rectangle.</summary>
    public int Left { get; set; }

    /// <summary>Specifies the <i>y</i>-coordinate of the upper-left corner of the rectangle.</summary>
    public int Top { get; set; }

    /// <summary>Specifies the <i>x</i>-coordinate of the lower-right corner of the rectangle.</summary>
    public int Right { get; set; }

    /// <summary>Specifies the <i>y</i>-coordinate of the lower-right corner of the rectangle.</summary>
    public int Bottom { get; set; }

    public int Width
    {
        get { return Right - Left; }
    }

    public int Height
    {
        get { return Bottom - Top; }
    }

    public Size Size
    {
        get { return new Size() { Height = Height, Width = Width }; }
    }
}
