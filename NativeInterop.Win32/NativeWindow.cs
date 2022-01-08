using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.WindowsAndMessaging;
using Microsoft.UI.Xaml;
using Microsoft.Win32.SafeHandles;

namespace NativeInterop.Win32
{
    public class NativeWindow
    {
        private IntPtr _hwnd;
        private IList<INativeWindowListener> _listeners = new List<INativeWindowListener>();
        private UISettings _uiSettings;

        private readonly static Windows.UI.Color White = Windows.UI.Color.FromArgb(
            255,
            255,
            255,
            255
        );

        private readonly static Windows.UI.Color Black = Windows.UI.Color.FromArgb(255, 0, 0, 0);

        public NativeWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                throw new NullReferenceException("The Window Handle is null.");
            }

            _hwnd = hwnd;

            SubClassingWin32();

            var settings = _uiSettings = new UISettings();
            settings.ColorValuesChanged += SettingsOnColorValuesChanged;
        }

        private void SettingsOnColorValuesChanged(UISettings sender, object args)
        {
            var fg = sender.GetColorValue(UIColorType.Foreground);
            var bg = sender.GetColorValue(UIColorType.Background);

            foreach (var listener in _listeners)
            {
                listener.OnSystemThemeChanged(this, fg, bg);
            }
        }

        private PInvoke.WinProc newWndProc = null!;
        private IntPtr oldWndProc = IntPtr.Zero;

        private bool _dragWindow = false;
        private POINT _lastPosition = new POINT() { x = 0, y = 0 };
        private bool _visible = true;

        public void AddListener(INativeWindowListener listener)
        {
            _listeners.Add(listener);
        }

        private void SubClassingWin32()
        {
            newWndProc = NewWindowProc;
            oldWndProc = PInvoke.SetWindowLongPtr(
                new HWND(_hwnd),
                WINDOW_LONG_PTR_INDEX.GWL_WNDPROC,
                Marshal.GetFunctionPointerForDelegate(newWndProc)
            );
        }

        public void StartDragging()
        {
            _dragWindow = true;
            PInvoke.ReleaseCapture();
            PInvoke.SendMessage(new HWND(_hwnd), PInvoke.WM_NCLBUTTONDOWN, PInvoke.HT_CAPTION, 0);
        }

        private IntPtr NewWindowProc(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                case WindowMessage.WM_LBUTTONUP:
                {
                    if (_dragWindow)
                    {
                        _dragWindow = false;
                    }
                    break;
                }
                case WindowMessage.WM_MOUSEMOVE:
                {
                    if (_dragWindow)
                    {
                        RECT windowRect;
                        PInvoke.GetWindowRect(new HWND(_hwnd), out windowRect);

                        POINT location;
                        PInvoke.GetCursorPos(out location);

                        int x = location.x - _lastPosition.x;
                        int y = location.y - _lastPosition.y;

                        PInvoke.MoveWindow(
                            new HWND(_hwnd),
                            x,
                            y,
                            windowRect.right - windowRect.left,
                            windowRect.bottom - windowRect.top,
                            false
                        );

                        _lastPosition = location;
                    }
                    break;
                }
                case WindowMessage.WM_ACTIVATE:
                    foreach (var listener in _listeners)
                    {
                        listener.Activated(this);
                    }
                    break;

                case WindowMessage.WM_GETMINMAXINFO:
                    MINMAXINFO minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                    minMaxInfo.ptMinTrackSize.x = (short)DisplayInformation.ConvertEpxToPixel(
                        hWnd,
                        MinWidth
                    );
                    minMaxInfo.ptMinTrackSize.y = (short)DisplayInformation.ConvertEpxToPixel(
                        hWnd,
                        MinHeight
                    );
                    if (MaxWidth > 0)
                    {
                        minMaxInfo.ptMaxTrackSize.x = (short)DisplayInformation.ConvertEpxToPixel(
                            hWnd,
                            MaxWidth
                        );
                    }

                    if (MaxHeight > 0)
                    {
                        minMaxInfo.ptMaxTrackSize.y = (short)DisplayInformation.ConvertEpxToPixel(
                            hWnd,
                            MaxHeight
                        );
                    }

                    Marshal.StructureToPtr(minMaxInfo, lParam, true);
                    break;

                case WindowMessage.WM_CLOSE:
                    if (!IsClosing)
                    {
                        IsClosing = true;
                        foreach (var listener in _listeners)
                        {
                            listener.Closing(this);
                        }
                    }
                    break;

                case WindowMessage.WM_MOVE:
                    foreach (var listener in _listeners)
                    {
                        listener.Moving(this);
                    }
                    break;
                case WindowMessage.WM_SIZING:
                    foreach (var listener in _listeners)
                    {
                        listener.Sizing(this);
                    }
                    break;
                case WindowMessage.WM_DPICHANGED:
                    foreach (var listener in _listeners)
                    {
                        listener.DpiChanged(this, HiWord(wParam));
                    }
                    break;
                case WindowMessage.WM_SETTINGCHANGE:
                    var ptrToStringAuto = Marshal.PtrToStringAnsi(lParam);
                    Debug.WriteLine(ptrToStringAuto);
                    if (ptrToStringAuto == "ImmersiveColorSet")
                    {
                        SettingsOnColorValuesChanged(_uiSettings, null);
                    }
                    break;
            }

            return PInvoke.CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }

        private static uint HiWord(IntPtr ptr)
        {
            uint value = (uint)(int)ptr;
            if ((value & 0x80000000) == 0x80000000)
                return (value >> 16);
            else
                return (value >> 16) & 0xffff;
        }

        public Rectangle RawBounds
        {
            get
            {
                RECT rect;
                Windows.Win32.PInvoke.GetWindowRect(new HWND(_hwnd), out rect);

                return new Rectangle
                {
                    Bottom = rect.bottom,
                    Left = rect.left,
                    Right = rect.right,
                    Top = rect.top
                };
            }
        }

        public Size RawSize
        {
            get { return RawBounds.Size; }
            set { SetRawSize(value.Width, value.Height); }
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

        private void SetRawSize(int width, int height)
        {
            var bounds = RawBounds;

            Windows.Win32.PInvoke.MoveWindow(
                new HWND(_hwnd),
                bounds.Left,
                bounds.Top,
                width,
                height,
                true
            );
        }

        public int RawWidth
        {
            get { return RawBounds.Width; }
            set { SetRawSize(value, RawHeight); }
        }

        public int RawHeight
        {
            get { return RawBounds.Height; }
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
                Windows.Win32.PInvoke.ShowWindow(
                    new HWND(_hwnd),
                    value ? SHOW_WINDOW_CMD.SW_SHOW : SHOW_WINDOW_CMD.SW_HIDE
                );
            }
        }

        public uint Dpi
        {
            get { return Windows.Win32.PInvoke.GetDpiForWindow(new HWND(_hwnd)); }
        }

        public int MinWidth { get; set; }
        public int MinHeight { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }

        public void HideMinimizeAndMaximizeButtons()
        {
            CurrentWindowStyles =
                CurrentWindowStyles
                & ~WindowStyles.WS_SIZEFRAME
                & ~WindowStyles.WS_MAXIMIZEBOX
                & ~WindowStyles.WS_MINIMIZEBOX;
        }

        public WindowStyles CurrentWindowStyles
        {
            get
            {
                return (WindowStyles)Windows.Win32.PInvoke.GetWindowLong(
                    new HWND(_hwnd),
                    WINDOW_LONG_PTR_INDEX.GWL_STYLE
                );
            }
            set
            {
                Windows.Win32.PInvoke.SetWindowLong(
                    new HWND(_hwnd),
                    WINDOW_LONG_PTR_INDEX.GWL_STYLE,
                    (int)(value)
                );
            }
        }

        public void HideCaptionAndBorder()
        {
            CurrentWindowStyles = WindowStyles.WS_POPUPWINDOW;
        }

        public void SetTopMost(bool isTopMost)
        {
            var rect = RawBounds;

            Windows.Win32.PInvoke.SetWindowPos(
                new HWND(_hwnd),
                new HWND((nint)(isTopMost ? ZOrder.HWND_TOPMOST : ZOrder.HWND_NOTOPMOST)),
                rect.Left,
                rect.Top,
                rect.Width,
                rect.Height,
                SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW
            );
        }

        public Color TitleBarColor
        {
            get => GetTitlebarBackgroundColor();
            set => SetTitlebarBackgroundColor(value);
        }

        public bool IsClosing { get; private set; }

        public Microsoft.UI.Xaml.ApplicationTheme CurrentSystemTheme
        {
            get
            {
                var fg = _uiSettings.GetColorValue(UIColorType.Foreground);
                var bg = _uiSettings.GetColorValue(UIColorType.Background);

                if (fg == White && bg == Black)
                {
                    return Microsoft.UI.Xaml.ApplicationTheme.Dark;
                }
                else if (fg == Black && bg == White)
                {
                    return Microsoft.UI.Xaml.ApplicationTheme.Light;
                }
                else
                {
                    // if a custom theme is active, fall back to "light" theme
                    return ApplicationTheme.Light;
                }
            }
        }

        const int DWMWA_CAPTION_COLOR = 35;

        public unsafe Color GetTitlebarBackgroundColor()
        {
            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf<COLORREF>());

            try
            {
                PInvoke.DwmGetWindowAttribute(
                    new HWND(_hwnd),
                    (DWMWINDOWATTRIBUTE)DWMWA_CAPTION_COLOR,
                    (void*)pnt,
                    sizeof(uint)
                );

                return Marshal.PtrToStructure<COLORREF>(pnt).GetColor();
            }
            finally
            {
                Marshal.FreeHGlobal(pnt);
            }
        }

        public unsafe void SetTitlebarBackgroundColor(Color titlebarBackgroundColor)
        {
            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf<COLORREF>());

            try
            {
                var nativeColor = new COLORREF(titlebarBackgroundColor);

                Marshal.StructureToPtr(nativeColor, pnt, false);

                PInvoke.DwmSetWindowAttribute(
                    new HWND(_hwnd),
                    (DWMWINDOWATTRIBUTE)DWMWA_CAPTION_COLOR,
                    (void*)pnt,
                    sizeof(uint)
                );
            }
            finally
            {
                Marshal.FreeHGlobal(pnt);
            }
        }

        public void LoadIcon(string iconName)
        {
            SafeFileHandle hIcon = Windows.Win32.PInvoke.LoadImage(
                new SafeFileHandle(IntPtr.Zero, false),
                iconName,
                GDI_IMAGE_TYPE.IMAGE_ICON,
                16,
                16,
                IMAGE_FLAGS.LR_LOADFROMFILE
            );

            Windows.Win32.PInvoke.SendMessage(
                new HWND(_hwnd),
                (uint)WindowMessage.WM_SETICON,
                0,
                hIcon.DangerousGetHandle()
            );

            hIcon.SetHandleAsInvalid();
        }

        public void Maximize()
        {
            Windows.Win32.PInvoke.ShowWindow(new HWND(_hwnd), SHOW_WINDOW_CMD.SW_MAXIMIZE);
        }

        public void Minimize()
        {
            Windows.Win32.PInvoke.ShowWindow(new HWND(_hwnd), SHOW_WINDOW_CMD.SW_MINIMIZE);
        }

        public void Restore()
        {
            Windows.Win32.PInvoke.ShowWindow(new HWND(_hwnd), SHOW_WINDOW_CMD.SW_RESTORE);
        }

        public void Hide()
        {
            Windows.Win32.PInvoke.ShowWindow(new HWND(_hwnd), SHOW_WINDOW_CMD.SW_HIDE);
        }

        public void BringToTop()
        {
            Windows.Win32.PInvoke.SetWindowPos(
                new HWND(_hwnd),
                (HWND)(int)ZOrder.HWND_TOPMOST,
                0,
                0,
                0,
                0,
                SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE
            );
        }
    }
}
