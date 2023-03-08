using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

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

    public static Size GetMonitorSizeFromWindow(IntPtr hWnd)
    {
        var id = PInvoke.MonitorFromWindow(
            new HWND(hWnd),
            MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
        );

        MONITORINFO info = default;
        info.cbSize = (uint)Marshal.SizeOf<MONITORINFO>();
        if (PInvoke.GetMonitorInfo(id, ref info))
        {
            return new Size(
                info.rcWork.right - info.rcWork.left,
                info.rcWork.bottom - info.rcWork.top
            );
        }

        throw new Exception("Failed to get monitor info");
    }
}
