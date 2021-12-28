using System.Drawing;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Windows.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct COLORREF
    {
        internal uint ColorDWORD;

        internal COLORREF(Color color)
        {
            ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
        }

        internal COLORREF(uint r, uint g, uint b)
        {
            ColorDWORD = r + (g << 8) + (b << 16);
        }

        internal Color GetColor()
        {
            return Color.FromArgb(
                (int)(0x000000FFU & ColorDWORD),
                (int)(0x0000FF00U & ColorDWORD) >> 8,
                (int)(0x00FF0000U & ColorDWORD) >> 16
            );
        }

        internal void SetColor(Color color)
        {
            ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
        }
    }
}