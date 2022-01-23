using PackageInstaller.Core.ModelViews;
using ReactiveUI;

namespace PackageInstaller.Pages;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name",
    Justification = "Workaround: Xaml doesn't support generic types.")]
public class ReactivePagePreparation : ReactivePage<PreparationViewModel>
{
}

public sealed partial class Preparation
{
    public Preparation()
    {
        this.InitializeComponent();
    }
}
