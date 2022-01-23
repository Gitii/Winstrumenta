using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PackageInstaller.AttachedProperties;

public class ImageExtensions : DependencyObject
{
    public static ImageSource? GetImageSourceFromUri(string? uriPath)
    {
        uriPath = uriPath?.Trim().Trim('\\', '/'); // clean path

        if (string.IsNullOrEmpty(uriPath))
        {
            return null;
        }

        var uri = new Uri($"ms-appx:///{uriPath}");

        if (uri.AbsolutePath.EndsWith(".svg", true, null))
        {
            var source = new SvgImageSource(uri);
            source.OpenFailed += SourceOnOpenFailed;
            return source;
        }
        else
        {
            var source = new BitmapImage(uri);
            source.ImageFailed += OnImageLoadFailed;
            return source;
        }
    }

    public static readonly DependencyProperty SourceUriProperty =
        DependencyProperty.RegisterAttached(
            "SourceUri",
            typeof(string),
            typeof(ImageExtensions),
            new PropertyMetadata(default(string), OnSourceUriChanged)
        );

    private static void OnSourceUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Image image)
        {
            if (e.NewValue == null)
            {
                image.Source = null;
            }
            else if (e.NewValue is string uriPath)
            {
                image.Source = GetImageSourceFromUri(uriPath);
            }
            else
            {
                // invalid new value, just clear image
                image.Source = null;
            }
        }
    }

    public static void SetSourceUri(Image element, string value)
    {
        element.SetValue(SourceUriProperty, value);
    }

    public static string GetSourceUri(Image element)
    {
        return (string)element.GetValue(SourceUriProperty);
    }

    private static void OnImageLoadFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
    }

    private static void SourceOnOpenFailed(
        SvgImageSource sender,
        SvgImageSourceFailedEventArgs args
    )
    {
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
    }
}
