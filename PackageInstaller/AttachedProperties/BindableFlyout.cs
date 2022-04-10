using System;
using System.Collections;
using Microsoft.UI.Xaml;

namespace PackageInstaller.AttachedProperties;

public class BindableFlyout : DependencyObject
{
    public static IEnumerable? GetItemsSource(DependencyObject obj)
    {
        return obj.GetValue(ItemsSourceProperty) as IEnumerable;
    }

    public static void SetItemsSource(DependencyObject obj, IEnumerable value)
    {
        obj.SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.RegisterAttached(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(BindableFlyout),
            new PropertyMetadata(null, ItemsSourceChanged)
        );

    private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Setup(d as Microsoft.UI.Xaml.Controls.Flyout);
    }

    public static DataTemplate? GetItemTemplate(DependencyObject obj)
    {
        return (DataTemplate)obj.GetValue(ItemTemplateProperty);
    }

    public static void SetItemTemplate(DependencyObject obj, DataTemplate? value)
    {
        obj.SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.RegisterAttached(
            "ItemTemplate",
            typeof(DataTemplate),
            typeof(BindableFlyout),
            new PropertyMetadata(null, ItemsTemplateChanged)
        );

    private static void ItemsTemplateChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        Setup(d as Microsoft.UI.Xaml.Controls.Flyout);
    }

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
    private static async void Setup(Microsoft.UI.Xaml.Controls.Flyout? m)
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
    {
        if (Windows.ApplicationModel.DesignMode.DesignModeEnabled || m == null)
        {
            return;
        }

        var s = GetItemsSource(m);
        if (s == null)
        {
            return;
        }

        var t = GetItemTemplate(m);
        if (t == null)
        {
            return;
        }

        var c = new Microsoft.UI.Xaml.Controls.ItemsControl { ItemsSource = s, ItemTemplate = t, };
        var n = Windows.UI.Core.CoreDispatcherPriority.Normal;
        Windows.UI.Core.DispatchedHandler h = () => m.Content = c;
        await m.Dispatcher.RunAsync(n, h);
    }
}
