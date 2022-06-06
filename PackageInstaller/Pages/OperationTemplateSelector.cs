using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PackageInstaller.Core.ModelViews;

namespace PackageInstaller.Pages;

public class OperationTemplateSelector : DataTemplateSelector
{
    public DataTemplate? RunningOperationTemplate { get; set; }

    public DataTemplate? FinishedOperationTemplate { get; set; }

    public DataTemplate? FailedOperationTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is OperationProgressModelView opmv)
        {
            if (opmv.HasFailed)
            {
                return FailedOperationTemplate!;
            }

            if (opmv.IsFinished)
            {
                return FinishedOperationTemplate!;
            }

            return RunningOperationTemplate!;
        }

        return RunningOperationTemplate!;
    }
}
