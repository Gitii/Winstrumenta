using System.Reactive.Disposables;
using Numbers.Core.ModelViews;
using ReactiveUI;

namespace Numbers.Controls;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "MA0048:File name must match type name",
    Justification = "Workaround: Xaml doesn't support generic types."
)]
public class ReactiveControlRecentlyUsedFileView
    : ReactiveUserControl<RecentlyUsedFileViewModel> { }

public sealed partial class RecentlyUsedFileView
{
    public RecentlyUsedFileView()
    {
        this.InitializeComponent();

        this.WhenActivated(
            (disposable) =>
            {
                this.OneWayBind(ViewModel, (vm) => vm.FileName, (v) => v.TitleField.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, (vm) => vm.FormatedAccessDate, (v) => v.DateField.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, (vm) => vm.Path, (v) => v.PathField.Text)
                    .DisposeWith(disposable);
                this.BindCommand(ViewModel, (vm) => vm.OpenDocument, (v) => v.Button)
                    .DisposeWith(disposable);
            }
        );
    }
}
