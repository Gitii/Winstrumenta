using System;

namespace WindowExtensions;

public class DisplayInfo
{
    public string Availability { get; set; }
    public int ScreenHeight { get; set; }
    public int ScreenWidth { get; set; }

    public int ScreenEfectiveHeight
    {
        get
        {
            int widthDPI;
            _ = PInvoke.SHCore.GetDpiForMonitor(
                hMonitor,
                PInvoke.MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI,
                out widthDPI,
                out _
            );
            float scalingFactor = (float)widthDPI / 96;
            return (int)(ScreenHeight / scalingFactor);
        }
    }

    public int ScreenEfectiveWidth
    {
        get
        {
            int heightDPI;
            _ = PInvoke.SHCore.GetDpiForMonitor(
                hMonitor,
                PInvoke.MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI,
                out _,
                out heightDPI
            );
            float scalingFactor = (float)heightDPI / 96;
            return (int)(ScreenWidth / scalingFactor);
        }
    }

    public string DeviceName { get; set; }
    public PInvoke.RECT WorkArea { get; set; }
    public IntPtr hMonitor { get; set; }
}
