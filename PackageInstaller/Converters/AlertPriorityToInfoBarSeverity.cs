using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using PackageInstaller.Core.Services;

namespace PackageInstaller.Converters;

public class AlertPriorityToInfoBarSeverity : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DistributionList.AlertPriority prio)
        {
            return prio switch
            {
                DistributionList.AlertPriority.Important => InfoBarSeverity.Warning,
                DistributionList.AlertPriority.Information => InfoBarSeverity.Informational,
                DistributionList.AlertPriority.Critical => InfoBarSeverity.Error,
                _ => InfoBarSeverity.Informational
            };
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
