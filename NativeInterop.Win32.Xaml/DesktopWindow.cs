using System;
using System.Runtime.InteropServices;
using Windows.UI;
using Microsoft.UI.Xaml;
using WinRT;

namespace NativeInterop.Win32.Xaml;

public class DesktopWindow : Window, INativeWindowListener
{
    private IntPtr _hwnd = IntPtr.Zero;
    private string? _iconResource;
    private bool _loaded = false;
    protected NativeWindow _nativeWindow;
    private DynamicApplicationTheme _requestedTheme = DynamicApplicationTheme.Auto;

    public DesktopWindow()
    {
        _hwnd = this.As<IWindowNative>().WindowHandle;
        if (_hwnd == IntPtr.Zero)
        {
            throw new Exception("The Window Handle is null.");
        }

        _nativeWindow = new NativeWindow(_hwnd);
        _nativeWindow.AddListener(this);
    }

    public int Width
    {
        get => _nativeWindow.Width;
        set => _nativeWindow.Width = value;
    }

    public int Height
    {
        get => _nativeWindow.Height;
        set => _nativeWindow.Height = value;
    }

    public int MinWidth
    {
        get => _nativeWindow.MinWidth;
        set => _nativeWindow.MinWidth = value;
    }

    public int MinHeight
    {
        get => _nativeWindow.MinHeight;
        set => _nativeWindow.MinHeight = value;
    }

    public int MaxWidth
    {
        get => _nativeWindow.MaxWidth;
        set => _nativeWindow.MaxWidth = value;
    }

    public int MaxHeight
    {
        get => _nativeWindow.MaxHeight;
        set => _nativeWindow.MaxHeight = value;
    }

    public bool IsClosing
    {
        get => _nativeWindow.IsClosing;
    }

    public new UIElement Content
    {
        get => ((Window)this).Content;
        set
        {
            ((Window)this).Content = value;
            RequestedTheme = RequestedTheme;
        }
    }

    public DynamicApplicationTheme RequestedTheme
    {
        get { return _requestedTheme; }
        set
        {
            _requestedTheme = value;

            if (_requestedTheme == DynamicApplicationTheme.Auto)
            {
                ApplyTheme(_nativeWindow.CurrentSystemTheme);
            }
            else if (_requestedTheme == DynamicApplicationTheme.Dark)
            {
                ApplyTheme(ApplicationTheme.Dark);
            }
            else
            {
                ApplyTheme(ApplicationTheme.Light);
            }
        }
    }

    public ApplicationTheme ActualTheme
    {
        get
        {
            if (Content is FrameworkElement elem)
            {
                var elementTheme = elem.RequestedTheme;
                if (elementTheme == ElementTheme.Light)
                {
                    return ApplicationTheme.Light;
                }
                else if (elementTheme == ElementTheme.Dark)
                {
                    return ApplicationTheme.Dark;
                }
                else
                {
                    return Application.Current.RequestedTheme;
                }
            }

            return Application.Current.RequestedTheme;
        }
    }

    public IntPtr Hwnd
    {
        get { return _hwnd; }
    }

    public uint Dpi
    {
        get { return _nativeWindow.Dpi; }
    }

    public string? Icon
    {
        get => _iconResource;
        set
        {
            _iconResource = value;

            if (_iconResource != null)
            {
                _nativeWindow.LoadIcon(_iconResource);
            }
        }
    }

    public bool Resizeable
    {
        get => _nativeWindow.CurrentWindowStyles.HasFlag(WindowStyles.WS_THICKFRAME);
        set
        {
            if (value)
            {
                _nativeWindow.CurrentWindowStyles |= WindowStyles.WS_THICKFRAME;
            }
            else
            {
                _nativeWindow.CurrentWindowStyles &= ~WindowStyles.WS_THICKFRAME;
            }
        }
    }

    public void OnActivated(NativeWindow nativeWindow)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        Loaded?.Invoke(this, new WindowLoadedEventArgs(this));
    }

    void INativeWindowListener.Closing(NativeWindow nativeWindow)
    {
        Closing?.Invoke(this, new WindowClosingEventArgs(this));
    }

    void INativeWindowListener.Moving(NativeWindow nativeWindow)
    {
        OnWindowMoving();
    }

    void INativeWindowListener.Sizing(NativeWindow nativeWindow)
    {
        OnWindowSizing();
    }

    void INativeWindowListener.DpiChanged(NativeWindow nativeWindow, uint dpi)
    {
        OnWindowDpiChanged(dpi);
    }

    public void OnSystemThemeChanged(NativeWindow nativeWindow, Color foreground, Color Background)
    {
        if (RequestedTheme == DynamicApplicationTheme.Auto)
        {
            ForceUpdateTheme();
        }
    }

    private void ApplyTheme(ApplicationTheme applicationTheme)
    {
        if (Content is FrameworkElement elem)
        {
            switch (applicationTheme)
            {
                case ApplicationTheme.Light:
                    elem.RequestedTheme = ElementTheme.Light;
                    break;
                case ApplicationTheme.Dark:
                    elem.RequestedTheme = ElementTheme.Dark;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(applicationTheme),
                        applicationTheme,
                        null
                    );
            }
        }
    }

    public event EventHandler<WindowClosingEventArgs>? Closing;
    public event EventHandler<WindowMovingEventArgs>? Moving;
    public event EventHandler<WindowSizingEventArgs>? Sizing;
    public event EventHandler<WindowDpiChangedEventArgs>? DpiChanged;
    public event EventHandler<WindowLoadedEventArgs>? Loaded;

    public void Maximize()
    {
        _nativeWindow.Maximize();
    }

    public void Minimize()
    {
        _nativeWindow.Minimize();
    }

    public void Restore()
    {
        _nativeWindow.Restore();
    }

    public void Hide()
    {
        _nativeWindow.Hide();
    }

    public void BringToTop()
    {
        _nativeWindow.BringToTop();
    }

    public void ForceUpdateTheme()
    {
        RequestedTheme = RequestedTheme;
    }

    private void OnClosing()
    {
        WindowClosingEventArgs windowClosingEventArgs = new(this);
        Closing?.Invoke(this, windowClosingEventArgs);
    }

    private void OnWindowMoving()
    {
        var windowPosition = _nativeWindow.RawBounds;
        //windowPosition comes in pixels(Win32), so you need to convert into epx
        WindowMovingEventArgs windowMovingEventArgs =
            new(
                this,
                new WindowPosition(
                    DisplayInformation.ConvertPixelToEpx(_hwnd, windowPosition.Top),
                    DisplayInformation.ConvertPixelToEpx(_hwnd, windowPosition.Left)
                )
            );

        Moving?.Invoke(this, windowMovingEventArgs);
    }

    private void OnWindowSizing()
    {
        WindowSizingEventArgs windowSizingEventArgs = new(this);
        Sizing?.Invoke(this, windowSizingEventArgs);
    }

    private void OnWindowDpiChanged(uint newDpi)
    {
        WindowDpiChangedEventArgs windowDpiChangedEvent = new(this, (int)newDpi);
        DpiChanged?.Invoke(this, windowDpiChangedEvent);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
    internal interface IWindowNative
    {
        IntPtr WindowHandle { get; }
    }
}
