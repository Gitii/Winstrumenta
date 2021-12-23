using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Windows.Sdk
{
    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public uint AccentFlags;
        public uint GradientColor;
        public uint AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }

    internal static partial class PInvoke
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(
            IntPtr hwnd,
            ref WindowCompositionAttributeData data
        );

        [DllImport("user32")]
        internal static extern IntPtr SetWindowLongPtr(
            IntPtr hWnd,
            Vanara.PInvoke.User32.WindowLongFlags flags,
            WinProc newProc
        );

        [DllImport("user32.dll")]
        internal static extern IntPtr CallWindowProc(
            IntPtr lpPrevWndFunc,
            IntPtr hWnd,
            Vanara.PInvoke.User32.WindowMessage Msg,
            IntPtr wParam,
            IntPtr lParam
        );

        public delegate IntPtr WinProc(
            IntPtr hWnd,
            Vanara.PInvoke.User32.WindowMessage Msg,
            IntPtr wParam,
            IntPtr lParam
        );

        [DllImport("user32.dll")]
        internal static extern int FillRect(
            IntPtr hDC,
            [In] ref Vanara.PInvoke.RECT lprc,
            IntPtr hbr
        );

        [DllImport("user32.dll")]
        internal extern static bool GetUpdateRect(
            IntPtr hWnd,
            ref Vanara.PInvoke.RECT rect,
            bool bErase
        );
    }
}
