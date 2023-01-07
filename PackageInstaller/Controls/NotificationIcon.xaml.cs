using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using PackageInstaller.Core.Services;
using Application = Microsoft.UI.Xaml.Application;
using UserControl = Microsoft.UI.Xaml.Controls.UserControl;

namespace PackageInstaller.Controls;

public sealed partial class NotificationIcon : UserControl
{
    public static readonly DependencyProperty PriorityProperty = DependencyProperty.Register(
        "Priority",
        typeof(DistributionList.AlertPriority),
        typeof(NotificationIcon),
        new PropertyMetadata(default(DistributionList.AlertPriority), PropertyChangedCallback)
    );

    private static void PropertyChangedCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is not NotificationIcon icon)
        {
            return;
        }

        DistributionList.AlertPriority prio = (DistributionList.AlertPriority)e.NewValue;

        var glyph = GetGlyph(prio);
        var (backgroundColor, foregroundColor) = GetColors(prio);

        icon.NotificationIconGlyphTop.Glyph = glyph;
        icon.NotificationIconGlyphBottom.Foreground = foregroundColor;
        icon.NotificationIconGlyphBottom.Foreground = backgroundColor;
    }

    private static (Brush, Brush) GetColors(DistributionList.AlertPriority prio)
    {
        return prio switch
        {
            DistributionList.AlertPriority.Critical
              => GetResources(
                  "InfoBarErrorSeverityIconBackground",
                  "InfoBarErrorSeverityIconForeground"
              ),
            DistributionList.AlertPriority.Important
              => GetResources(
                  "InfoBarWarningSeverityIconBackground",
                  "InfoBarWarningSeverityIconForeground"
              ),
            DistributionList.AlertPriority.Information
              => GetResources(
                  "InfoBarInformationalSeverityIconBackground",
                  "InfoBarInformationalSeverityIconForeground"
              ),
            _ => throw new ArgumentOutOfRangeException(nameof(prio), prio, null)
        };

        (Brush, Brush) GetResources(string key1, string key2)
        {
            return (
                (Brush)Application.Current.Resources[key1],
                (Brush)Application.Current.Resources[key2]
            );
        }
    }

    private static string GetGlyph(DistributionList.AlertPriority prio)
    {
        return prio switch
        {
            DistributionList.AlertPriority.Information => "\uF13F", // StatusCircleInfo
            DistributionList.AlertPriority.Important => "\uF13C", // StatusCircleExclamation
            DistributionList.AlertPriority.Critical => "\uF13D", // StatusCircleErrorX
            _ => throw new ArgumentOutOfRangeException(nameof(prio), prio, null)
        };
    }

    public DistributionList.AlertPriority Priority
    {
        get { return (DistributionList.AlertPriority)GetValue(PriorityProperty); }
        set { SetValue(PriorityProperty, value); }
    }

    public NotificationIcon()
    {
        this.InitializeComponent();
    }
}
