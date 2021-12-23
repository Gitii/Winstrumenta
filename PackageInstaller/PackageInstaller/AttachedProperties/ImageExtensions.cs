using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PackageInstaller.AttachedProperties
{
    public class ImageExtensions : DependencyObject
    {
        public static readonly DependencyProperty SourceUriProperty =
            DependencyProperty.RegisterAttached(
                "SourceUri",
                typeof(string),
                typeof(ImageExtensions),
                new PropertyMetadata(default(string), OnSourceUriChanged)
            );

        private static void OnSourceUriChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            if (d is Image image)
            {
                if (e.NewValue == null)
                {
                    image.Source = null;
                }
                else if (e.NewValue is string uriPath)
                {
                    var uri = new Uri($"ms-appx:///{uriPath}");

                    if (uri.AbsolutePath.EndsWith(".svg", true, null))
                    {
                        var source = new SvgImageSource(uri);
                        image.Source = source;
                    }
                    else
                    {
                        var source = new BitmapImage(uri);
                        source.ImageFailed += OnImageLoadFailed;
                        image.Source = source;
                    }
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
            if (Debugger.IsAttached) Debugger.Break();
        }

    }
}
