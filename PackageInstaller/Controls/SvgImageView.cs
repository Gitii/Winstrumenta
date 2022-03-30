// Source: https://github.com/mono/SkiaSharp/blob/master/source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKElement.cs

using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using Svg;
using Svg.Skia;
using Windows.Foundation;
using Windows.UI.Core;
using Microsoft.UI.Dispatching;
using DependencyProperty = Microsoft.UI.Xaml.DependencyProperty;
using Image = Microsoft.UI.Xaml.Controls.Image;
using PropertyMetadata = Microsoft.UI.Xaml.PropertyMetadata;

namespace PackageInstaller.Controls;

/// <summary>
/// An image view that renders svg images.
/// </summary>
[DefaultProperty("Name")]
public class SvgImageView : Grid
{
    private readonly Image _image;
    private bool _ignorePixelScaling = false;
    private Size _size = Size.Empty;
    private string _svg = String.Empty;
    private double _pixelScale = 1d;

    public SvgImageView()
    {
        _image = new Image();
        this.Children.Add(_image);
        _image.HorizontalAlignment = HorizontalAlignment.Stretch;
        _image.VerticalAlignment = VerticalAlignment.Stretch;
        _image.Stretch = Stretch.Uniform;

        RegisterPropertyChangedCallback(SourceProperty, OnSourceChanged);
        SizeChanged += OnSizeChanged;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        XamlRoot.Changed += XamlRootOnChanged;
        _pixelScale = XamlRoot.RasterizationScale;
    }

    private void XamlRootOnChanged(XamlRoot sender, XamlRootChangedEventArgs args)
    {
        _pixelScale = sender.RasterizationScale;
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        "Source",
        typeof(object),
        typeof(SvgImageView),
        new PropertyMetadata(default(object))
    );

    public object? Source
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
            if (svg.StartsWith("<", StringComparison.Ordinal))
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
            if (IsPngStream(stream))
            {
                try
                {
                    _image.DispatcherQueue.TryEnqueue(
                        DispatcherQueuePriority.Normal,
                        async () =>
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            var image = new BitmapImage();
                            await image.SetSourceAsync(stream.AsRandomAccessStream());
                            _image.Source = image;
                        }
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                return;
            }

            using var reader = new StreamReader(stream);
            _svg = reader.ReadToEnd();
        }
        else
        {
            throw new Exception("Unknown value");
        }

        UpdateImage();
    }

    private byte[] PNG_HEADER = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

    private bool IsPngStream(Stream stream)
    {
        Span<byte> header = stackalloc byte[8];
        if (stream.Read(header) != header.Length)
        {
            throw new Exception("Failed to read header");
        }

        return header.SequenceEqual(PNG_HEADER);
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

        Task.Run(Update)
            .ContinueWith(
                (task, _) =>
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
                },
                TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.FromCurrentSynchronizationContext()
            )
#pragma warning disable VSTHRD110 // Observe result of async calls
            .ContinueWith(
                (task) =>
                    throw new AggregateException(
                        "Failed to update image",
                        task.Exception ?? new Exception("Unknown failure reason")
                    ),
                TaskContinuationOptions.NotOnRanToCompletion
            );
#pragma warning restore VSTHRD110 // Observe result of async calls
    }

    private Stream? Update()
    {
        var bounds = GetImageBounds();
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return null;
        }

        using var svg = new SKSvg();
        SvgDocument svgDocument = SvgDocument.FromSvg<SvgDocument>(_svg);

        UpdateSize(svgDocument, bounds);

        using var svgImage =
            svg.FromSvgDocument(svgDocument)
            ?? throw new Exception("Failed to load svg from document");

        var buffer = new MemoryStream();
        var success = svgImage.ToImage(
            buffer,
            SKColor.Empty,
            SKEncodedImageFormat.Png,
            100,
            1,
            1,
            SKColorType.Rgba8888,
            SKAlphaType.Premul,
            SKColorSpace.CreateSrgb()
        );

        if (!success)
        {
            throw new Exception("Failed to render svg");
        }

        buffer.Position = 0;

        return buffer;
    }

    private void UpdateSize(SvgDocument svgDocument, Size bounds)
    {
        bool hasWidthAndHeight =
            svgDocument.ContainsAttribute("width") && svgDocument.ContainsAttribute("height");
        bool hasViewBox = svgDocument.ViewBox != SvgViewBox.Empty;
        Size imageSize;

        if (!hasViewBox && !hasWidthAndHeight)
        {
            throw new Exception("Invalid svg: neither width&height nor viewBox");
        }

        if (hasViewBox)
        {
            imageSize = new Size(svgDocument.ViewBox.Width, svgDocument.ViewBox.Height);
        }
        else
        {
            // width & height set
            imageSize = new Size(svgDocument.Width.Value, svgDocument.Height.Value);
        }

        if (!hasViewBox && hasWidthAndHeight)
        {
            svgDocument.ViewBox = new SvgViewBox(
                0,
                0,
                svgDocument.Width.Value,
                svgDocument.Height.Value
            );
        }

        Size optimalSize = FitInBounds(imageSize, bounds.Width, bounds.Height);

        // set width and height to enable scaling
        svgDocument.Width = (SvgUnit)optimalSize.Width;
        svgDocument.Height = (SvgUnit)optimalSize.Height;
    }

    private Size FitInBounds(Size imageSize, double maxWidth, double maxHeight)
    {
        var xRatio = maxWidth / imageSize.Width;
        var yRatio = maxHeight / imageSize.Height;

        var ratio = Math.Min(xRatio, yRatio);

        return new Size(
            Math.Min(maxWidth, imageSize.Width * ratio),
            Math.Min(maxHeight, imageSize.Height * ratio)
        );
    }

    private Size GetImageBounds()
    {
        var w = _size.Width;
        var h = _size.Height;

        if (!IsPositive(w) || !IsPositive(h))
        {
            return Size.Empty;
        }

        if (IgnorePixelScaling)
        {
            return new Size((int)w, (int)h);
        }

        return new Size((int)(w * _pixelScale), (int)(h * _pixelScale));

        bool IsPositive(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
        }
    }
}
