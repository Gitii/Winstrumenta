using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;
using Vanara.PInvoke;
using HWND = Vanara.PInvoke.HWND;
using RECT = Vanara.PInvoke.RECT;

namespace NativeInterop.Win32
{
    [Flags]
    public enum WindowStyles : uint
    {
        WS_BORDER = 0x800000,
        WS_CAPTION = 0xc00000,
        WS_CHILD = 0x40000000,
        WS_CLIPCHILDREN = 0x2000000,
        WS_CLIPSIBLINGS = 0x4000000,
        WS_DISABLED = 0x8000000,
        WS_DLGFRAME = 0x400000,
        WS_GROUP = 0x20000,
        WS_HSCROLL = 0x100000,
        WS_MAXIMIZE = 0x1000000,
        WS_MAXIMIZEBOX = 0x10000,
        WS_MINIMIZE = 0x20000000,
        WS_MINIMIZEBOX = 0x20000,
        WS_OVERLAPPED = 0x0,
        WS_OVERLAPPEDWINDOW =
            WS_OVERLAPPED
            | WS_CAPTION
            | WS_SYSMENU
            | WS_SIZEFRAME
            | WS_MINIMIZEBOX
            | WS_MAXIMIZEBOX,
        WS_POPUP = 0x80000000u,
        WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
        WS_SIZEFRAME = 0x40000,
        WS_SYSMENU = 0x80000,
        WS_TABSTOP = 0x10000,
        WS_VISIBLE = 0x10000000,
        WS_VSCROLL = 0x200000,
        WS_THICKFRAME = 0x00040000
    }

    enum WindoowZOrder : int
    {
        HWND_BOTTOM = 1,
        HWND_NOTOPMOST = -2,
        HWND_TOP = 0,
        HWND_TOPMOST = -1,
    }

    public readonly struct Size
    {
        public int Width { get; init; }

        public int Height { get; init; }
    }

    public class NativeWindow
    {
        private IntPtr _hwnd;

        public NativeWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                throw new NullReferenceException("The Window Handle is null.");
            }

            _hwnd = hwnd;

            SubClassingWin32();
        }

        private Microsoft.Windows.Sdk.PInvoke.WinProc newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;

        private bool _dragWindow = false;
        private Point _lastPosition = new Point(0, 0);
        private bool _visible = true;

        private unsafe void SubClassingWin32()
        {
            newWndProc = new PInvoke.WinProc(NewWindowProc);
            oldWndProc = Microsoft.Windows.Sdk.PInvoke.SetWindowLongPtr(
                _hwnd,
                User32.WindowLongFlags.GWL_WNDPROC,
                newWndProc
            );
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public void StartDragging()
        {
            // _dragWindow = true;

            // User32.GetCursorPos(out _lastPosition);

            ReleaseCapture();
            SendMessage(_hwnd, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private IntPtr NewWindowProc(
            IntPtr hWnd,
            User32.WindowMessage Msg,
            IntPtr wParam,
            IntPtr lParam
        )
        {
            if (_dragWindow)
            {
                switch (Msg)
                {
                    case User32.WindowMessage.WM_LBUTTONDOWN:
                    {
                        _dragWindow = true;
                        break;
                    }
                    case User32.WindowMessage.WM_LBUTTONUP:
                    {
                        _dragWindow = false;
                        break;
                    }
                    case User32.WindowMessage.WM_MOUSEMOVE:
                    {
                        if (_dragWindow)
                        {
                            RECT windowRect;
                            User32.GetWindowRect(_hwnd, out windowRect);

                            Point location;
                            User32.GetCursorPos(out location);

                            int x = location.X - _lastPosition.X;
                            int y = location.Y - _lastPosition.Y;
                            User32.MoveWindow(
                                _hwnd,
                                x,
                                y,
                                windowRect.Width,
                                windowRect.Height,
                                false
                            );

                            _lastPosition = location;
                        }
                        break;
                    }
                }
            }

            return Microsoft.Windows.Sdk.PInvoke.CallWindowProc(
                oldWndProc,
                hWnd,
                Msg,
                wParam,
                lParam
            );
        }

        public Size RawSize
        {
            get
            {
                Vanara.PInvoke.RECT rect;
                Vanara.PInvoke.User32.GetWindowRect(_hwnd, out rect);

                return new Size()
                {
                    Width = rect.right - rect.left,
                    Height = rect.bottom - rect.top
                };
            }
        }

        public Size Size
        {
            get
            {
                var rs = RawSize;

                return new Size()
                {
                    Width = DisplayInformation.ConvertPixelToEpx(_hwnd, rs.Width),
                    Height = DisplayInformation.ConvertPixelToEpx(_hwnd, rs.Height)
                };
            }
        }

        public void SetRawSize(int width, int height)
        {
            Vanara.PInvoke.RECT rect;
            Vanara.PInvoke.User32.GetWindowRect(_hwnd, out rect);

            Vanara.PInvoke.User32.MoveWindow(_hwnd, rect.left, rect.top, width, height, true);
        }

        public int RawWidth
        {
            get
            {
                Vanara.PInvoke.RECT rect;
                Vanara.PInvoke.User32.GetWindowRect(_hwnd, out rect);

                return rect.right - rect.left;
            }
            set { SetRawSize(value, RawHeight); }
        }

        public int RawHeight
        {
            get
            {
                Vanara.PInvoke.RECT rect;
                Vanara.PInvoke.User32.GetWindowRect(_hwnd, out rect);

                return rect.bottom - rect.top;
            }
            set { SetRawSize(RawWidth, value); }
        }

        public int Height
        {
            get { return DisplayInformation.ConvertPixelToEpx(_hwnd, RawHeight); }
            set { RawHeight = DisplayInformation.ConvertEpxToPixel(_hwnd, value); }
        }

        public int Width
        {
            get { return DisplayInformation.ConvertPixelToEpx(_hwnd, RawWidth); }
            set { RawWidth = DisplayInformation.ConvertEpxToPixel(_hwnd, value); }
        }

        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                Vanara.PInvoke.User32.ShowWindow(
                    _hwnd,
                    value ? ShowWindowCommand.SW_SHOW : ShowWindowCommand.SW_HIDE
                );
            }
        }

        public void HideMinimizeAndMaximizeButtons()
        {
            var currentStyle = (WindowStyles)Vanara.PInvoke.User32.GetWindowLong(
                _hwnd,
                User32.WindowLongFlags.GWL_STYLE
            );

            Vanara.PInvoke.User32.SetWindowLong(
                _hwnd,
                User32.WindowLongFlags.GWL_STYLE,
                (int)(
                    currentStyle
                    & ~WindowStyles.WS_SIZEFRAME
                    & ~WindowStyles.WS_MAXIMIZEBOX
                    & ~WindowStyles.WS_MINIMIZEBOX
                )
            );
        }

        public WindowStyles CurrentWindowStyles
        {
            get
            {
                return (WindowStyles)Vanara.PInvoke.User32.GetWindowLong(
                    _hwnd,
                    User32.WindowLongFlags.GWL_STYLE
                );
            }
            set
            {
                Vanara.PInvoke.User32.SetWindowLong(
                    _hwnd,
                    User32.WindowLongFlags.GWL_STYLE,
                    (int)(value)
                );
            }
        }

        public void HideCaptionAndBorder()
        {
            WindowStyles currentStyle = (WindowStyles)Vanara.PInvoke.User32.GetWindowLong(
                _hwnd,
                User32.WindowLongFlags.GWL_STYLE
            );

            Vanara.PInvoke.User32.SetWindowLong(
                _hwnd,
                User32.WindowLongFlags.GWL_STYLE,
                (IntPtr)(User32.WindowStyles.WS_POPUPWINDOW)
            );
        }

        public void SetTopMost(bool isTopMost)
        {
            Vanara.PInvoke.RECT rect;
            Vanara.PInvoke.User32.GetWindowRect(_hwnd, out rect);

            Vanara.PInvoke.User32.SetWindowPos(
                _hwnd,
                new HWND(
                    (nint)(isTopMost ? WindoowZOrder.HWND_TOPMOST : WindoowZOrder.HWND_NOTOPMOST)
                ),
                rect.left,
                rect.top,
                rect.right - rect.left,
                rect.bottom - rect.top,
                User32.SetWindowPosFlags.SWP_SHOWWINDOW
            );
        }

        public void CenterOnScreen()
        {
            var display = DisplayInformation.GetDisplay(_hwnd);

            var size = RawSize;

            Vanara.PInvoke.User32.MoveWindow(
                _hwnd,
                display.WorkArea.left + display.ScreenWidth / 2 - size.Width / 2,
                display.WorkArea.top + display.ScreenHeight / 2 - size.Height / 2,
                size.Width,
                size.Height,
                true
            );
        }

        public void EnableBlur(uint blurOpacity, uint blurBackgroundColor)
        {
            WindowComposition.EnableBlur(_hwnd, blurOpacity, blurBackgroundColor);
        }

        public void SetWindowTheme(string subAppName, string? subIdList = null)
        {
            Vanara.PInvoke.UxTheme.SetWindowTheme(_hwnd, subAppName, subIdList);
        }

        public void SetTitlebarBackgroundColor(Color titlebarBackgroundColor)
        {
            const int DWMWA_CAPTION_COLOR = 35;
            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf<COLORREF>());

            try
            {
                var nativeColor = new COLORREF()
                {
                    R = titlebarBackgroundColor.R,
                    G = titlebarBackgroundColor.G,
                    B = titlebarBackgroundColor.B
                };

                Marshal.StructureToPtr(nativeColor, pnt, false);

                DwmApi.DwmSetWindowAttribute(
                    _hwnd,
                    (DwmApi.DWMWINDOWATTRIBUTE)DWMWA_CAPTION_COLOR,
                    pnt,
                    sizeof(uint)
                );
            }
            finally
            {
                Marshal.FreeHGlobal(pnt);
            }
        }

        public void SetBlackBackgroundColor()
        {
            //var hdc = PInvoke.GetDC(_hwnd);
            //var blackBrush = PInvoke.GetStockObject(GetStockObject_iFlags.GRAY_BRUSH);
            //PInvoke.SetDCBrushColor(hdc, (uint) blackBrush.Value);
            //PInvoke.ReleaseDC(_hwnd, hdc);
            //RECT rect;
            //PInvoke.GetClientRect(_hwnd, out rect);
            //PInvoke.InvalidateRect(_hwnd, rect, true);
            //PInvoke.UpdateWindow(_hwnd);
        }
    }
}
