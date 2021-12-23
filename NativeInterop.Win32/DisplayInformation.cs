using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using HDC = Vanara.PInvoke.HDC;
using HMONITOR = Vanara.PInvoke.HMONITOR;
using HWND = Vanara.PInvoke.HWND;
using RECT = Vanara.PInvoke.RECT;

namespace NativeInterop.Win32
{
    public readonly struct Rectangle
    {
        /// <summary>Specifies the <i>x</i>-coordinate of the upper-left corner of the rectangle.</summary>
        public int left { get; init; }

        /// <summary>Specifies the <i>y</i>-coordinate of the upper-left corner of the rectangle.</summary>
        public int top { get; init; }

        /// <summary>Specifies the <i>x</i>-coordinate of the lower-right corner of the rectangle.</summary>
        public int right { get; init; }

        /// <summary>Specifies the <i>y</i>-coordinate of the lower-right corner of the rectangle.</summary>
        public int bottom { get; init; }
    }

    public class DisplayInfo
    {
        public string Availability { get; set; }
        public int ScreenHeight { get; set; }
        public int ScreenWidth { get; set; }

        public string DeviceName { get; set; }
        public Rectangle WorkArea { get; set; }
        public IntPtr hMonitor { get; set; }
    }

    unsafe public class DisplayInformation
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
            var dpi = Vanara.PInvoke.User32.GetDpiForWindow(new HWND(hwnd));
            float scalingFactor = (float)dpi / 96;
            return scalingFactor;
        }

        public static DisplayInfo GetDisplay(IntPtr hwnd)
        {
            DisplayInfo di = null;
            Vanara.PInvoke.HMONITOR hMonitor;
            RECT rc;
            Vanara.PInvoke.User32.GetWindowRect(new HWND(hwnd), out rc);
            hMonitor = Vanara.PInvoke.User32.MonitorFromRect(
                in rc,
                User32.MonitorFlags.MONITOR_DEFAULTTONEAREST
            );

            var mi = new User32.MONITORINFO();
            mi.cbSize = (uint)Marshal.SizeOf<User32.MONITORINFO>();
            bool success = Vanara.PInvoke.User32.GetMonitorInfo(hMonitor, ref mi);
            if (success)
            {
                di = ConvertMonitorInfoToDisplayInfo((IntPtr)hMonitor, mi);
            }
            return di;
        }

        private static DisplayInfo ConvertMonitorInfoToDisplayInfo(
            IntPtr hMonitor,
            Vanara.PInvoke.User32.MONITORINFO mi
        )
        {
            return new DisplayInfo
            {
                ScreenWidth = mi.rcMonitor.right - mi.rcMonitor.left,
                ScreenHeight = mi.rcMonitor.bottom - mi.rcMonitor.top,
                DeviceName = "",
                WorkArea = new Rectangle()
                {
                    top = mi.rcWork.top,
                    bottom = mi.rcWork.bottom,
                    left = mi.rcWork.left,
                    right = mi.rcWork.right
                },
                Availability = mi.dwFlags.ToString(),
                hMonitor = hMonitor
            };
        }

        unsafe public static List<DisplayInfo> GetDisplays()
        {
            List<DisplayInfo> col = new();

            _ = Vanara.PInvoke.User32.EnumDisplayMonitors(
                new HDC(IntPtr.Zero),
                default,
                delegate(IntPtr hMonitor, IntPtr hdcMonitor, PRECT lprcMonitor, IntPtr dwData)
                {
                    Vanara.PInvoke.User32.MONITORINFO mi = new Vanara.PInvoke.User32.MONITORINFO();
                    mi.cbSize = (uint)Marshal.SizeOf<Vanara.PInvoke.User32.MONITORINFO>();
                    bool success = Vanara.PInvoke.User32.GetMonitorInfo(hMonitor, ref mi);
                    if (success)
                    {
                        DisplayInfo di = ConvertMonitorInfoToDisplayInfo((IntPtr)hMonitor, mi);
                        col.Add(di);
                    }
                    return true;
                },
                IntPtr.Zero
            );
            return col;
        }

        public enum UserInteractionModeEnum
        {
            Touch,
            Mouse
        };
        public static UserInteractionModeEnum UserInteractionMode
        {
            get
            {
                // TODO: Have a counterpart event listeining the message WM_SETTINGCHANGE
                UserInteractionModeEnum userInteractionMode = UserInteractionModeEnum.Mouse;

                int state = Vanara.PInvoke.User32.GetSystemMetrics(
                    User32.SystemMetric.SM_CONVERTIBLESLATEMODE
                ); //O for tablet
                if (state == 0)
                {
                    userInteractionMode = UserInteractionModeEnum.Touch;
                }
                return userInteractionMode;
            }
        }
    }
}
