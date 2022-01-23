#region

using System;
using Windows.Win32;
using Windows.Win32.Foundation;

#endregion

namespace NativeInterop.Win32;

public static class DisplayInformation
{
    public static int ConvertEpxToPixel(IntPtr hwnd, int effectivePixels)
    {
        float scalingFactor = GetScalingFactor(hwnd);
        return (int)(effectivePixels * scalingFactor);
    }

    public static int ConvertPixelToEpx(IntPtr hwnd, int pixels)
    {
        float scalingFactor = GetScalingFactor(hwnd);
        return (int)(pixels / scalingFactor);
    }

    public static float GetScalingFactor(IntPtr hwnd)
    {
        var dpi = PInvoke.GetDpiForWindow(new HWND(hwnd));
        float scalingFactor = (float)dpi / 96;
        return scalingFactor;
    }
}
