// https://github.com/mono/SkiaSharp/blob/master/source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using Svg;
using Svg.Skia;
using Windows.Foundation;
using DependencyProperty = Microsoft.UI.Xaml.DependencyProperty;
using Image = Microsoft.UI.Xaml.Controls.Image;
using PropertyMetadata = Microsoft.UI.Xaml.PropertyMetadata;

namespace PackageInstaller.Controls;

[DefaultProperty("Name")]
public class SKElement : Grid
{
    private Image _image;
    private bool _ignorePixelScaling = false;
    private Windows.Foundation.Size _size = Windows.Foundation.Size.Empty;
    private string _svg = String.Empty;

    public SKElement()
    {
        _image = new Image();
        this.Children.Add(_image);
        _image.HorizontalAlignment = HorizontalAlignment.Stretch;
        _image.VerticalAlignment = VerticalAlignment.Stretch;
        _image.Stretch = Stretch.Uniform;

        RegisterPropertyChangedCallback(SourceProperty, OnSourceChanged);
        SizeChanged += OnSizeChanged;
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        "Source", typeof(object), typeof(SKElement), new PropertyMetadata(default(object)));

    public object Source
    {
        get { return (object)GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }

    private void OnSourceChanged(DependencyObject sender, DependencyProperty dp)
    {
        if (Source == null)
        {
            _svg = String.Empty;
        }
        else if (Source is string svg)
        {
            if (svg.StartsWith("<"))
            {
                // is svg document
                _svg = svg;
            }
            else
            {
                throw new Exception("Source is assigned a string but it's not a svg document");
            }
        }
        else if (Source is Stream stream)
        {
            using var reader = new StreamReader(stream);
            _svg = reader.ReadToEnd();
        }
        else
        {
            throw new Exception("Unknown value");
        }

        UpdateImage();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _size = e.NewSize;
        UpdateImage();
    }

    public bool IgnorePixelScaling
    {
        get { return _ignorePixelScaling; }
        set
        {
            var refresh = _ignorePixelScaling != value;
            _ignorePixelScaling = value;

            if (refresh)
            {
                UpdateImage();
            }
        }
    }

    private void UpdateImage()
    {
        if (_size.IsEmpty || string.IsNullOrEmpty(_svg))
        {
            _image.Source = null;

            return;
        }

        Task.Run(UpdateAsync)
            .ContinueWith((task, _) =>
                {
                    var buffer = task.Result;
                    if (buffer == null)
                    {
                        _image.Source = null;
                    }
                    else
                    {
                        var imageSource = new BitmapImage();
                        imageSource.SetSource(buffer.AsRandomAccessStream());

                        _image.Source = imageSource;
                        buffer.Close();
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.FromCurrentSynchronizationContext())
            .ContinueWith(
                (task) => throw new AggregateException("Failed to update image",
                    task.Exception ?? new Exception("Unknown failure reason")),
                TaskContinuationOptions.NotOnRanToCompletion);
    }

    private Stream? UpdateAsync()
    {
        var bounds = GetImageBounds();
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return null;

        using var svg = new SKSvg();
        SvgDocument svgDocument = SvgDocument.FromSvg<SvgDocument>(_svg);

        bool hasWidthAndHeight = svgDocument.ContainsAttribute("width") && svgDocument.ContainsAttribute("height");
        bool hasViewBox = svgDocument.ViewBox != SvgViewBox.Empty;
        Size imageSize;

        if (!hasViewBox && !hasWidthAndHeight)
        {
            throw new Exception("Invalid svg: neither width&height nor viewBox");
        }

        if (hasViewBox)
        {
            imageSize = new Windows.Foundation.Size(svgDocument.ViewBox.Width, svgDocument.ViewBox.Height);
        }
        else
        {
            // width & height set
            imageSize = new Size(svgDocument.Width, svgDocument.Height);
        }

        if (!hasViewBox && hasWidthAndHeight)
        {
            svgDocument.ViewBox = new SvgViewBox(0, 0, svgDocument.Width, svgDocument.Height);
        }

        Size optimalSize = FitInBounds(imageSize, bounds.Width, bounds.Height);

        // set width and height to enable scaling
        svgDocument.Width = (SvgUnit)optimalSize.Width;
        svgDocument.Height = (SvgUnit)optimalSize.Height;

        using var svgImage = svg.FromSvgDocument(svgDocument) ??
                             throw new Exception("Failed to load svg from document");

        var buffer = new MemoryStream();
        var success = svgImage.ToImage(buffer, SKColor.Empty, SKEncodedImageFormat.Png, 100, 1, 1,
            SKColorType.Rgba8888,
            SKAlphaType.Premul, SKColorSpace.CreateSrgb());

        if (!success)
        {
            throw new Exception("Failed to render svg");
        }

        buffer.Position = 0;

        return buffer;
    }

    private Size FitInBounds(Size imageSize, double maxWidth, double maxHeight)
    {
        var xRatio = maxWidth / imageSize.Width;
        var yRatio = maxHeight / imageSize.Height;

        var ratio = Math.Min(xRatio, yRatio);

        return
            new Size(Math.Min(maxWidth, imageSize.Width * ratio),
                Math.Min(maxHeight, imageSize.Height * ratio));
    }

    private Windows.Foundation.Size GetImageBounds()
    {
        var w = _size.Width;
        var h = _size.Height;

        if (!IsPositive(w) || !IsPositive(h))
            return Windows.Foundation.Size.Empty;

        if (IgnorePixelScaling)
            return new Windows.Foundation.Size((int)w, (int)h);

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(Program.MainWindow);

        var scale = NativeMethods.GetDpiForWindow(hwnd) / 96.0f;

        return new Windows.Foundation.Size((int)(w * scale), (int)(h * scale));

        bool IsPositive(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
        }
    }

    static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern int GetDpiForWindow(IntPtr hWnd);
    }
}