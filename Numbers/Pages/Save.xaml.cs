using System.Reactive.Disposables;
using Numbers.Core.ModelViews;
using ReactiveUI;

namespace Numbers.Pages;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "MA0048:File name must match type name",
    Justification = "Workaround: Xaml doesn't support generic types."
)]
public class ReactivePageSave : ReactivePage<SaveViewModel> { }

public partial class Save
{
    public Save()
    {
        InitializeComponent();

        this.WhenActivated(
            (disposable) =>
            {
                this.Bind(ViewModel, (vm) => vm.Delimiter, (v) => v.Delimiter.Text)
                    .DisposeWith(disposable);
                this.Bind(ViewModel, (vm) => vm.QuoteCharacter, (v) => v.Quote.Text)
                    .DisposeWith(disposable);
                this.Bind(ViewModel, (vm) => vm.Encoding, (v) => v.FileEncoding.SelectedItem)
                    .DisposeWith(disposable);
                this.Bind(ViewModel, (vm) => vm.FileName, (v) => v.FileName.Text)
                    .DisposeWith(disposable);
                this.Bind(ViewModel, (vm) => vm.FilePath, (v) => v.Directory.Text)
                    .DisposeWith(disposable);

                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.DelimiterOptions,
                        (v) => v.Delimiter.ItemsSource
                    )
                    .DisposeWith(disposable);
                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.EncodingOptions,
                        (v) => v.FileEncoding.ItemsSource
                    )
                    .DisposeWith(disposable);

                this.BindCommand(ViewModel, (vm) => vm.SaveCommand, (v) => v.SaveFile)
                    .DisposeWith(disposable);
                this.BindCommand(ViewModel, (vm) => vm.SaveAsCommand, (v) => v.SaveFileAs)
                    .DisposeWith(disposable);
            }
        );
    }
}
