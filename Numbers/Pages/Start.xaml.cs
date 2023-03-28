using System;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Numbers.Core.ModelViews;
using ReactiveUI;
using Windows.Storage.Pickers;

namespace Numbers.Pages;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "MA0048:File name must match type name",
    Justification = "Workaround: Xaml doesn't support generic types."
)]
public class ReactivePageStart : ReactivePage<StartViewModel> { }

public sealed partial class Start
{
    public Start()
    {
        this.InitializeComponent();

        this.WhenActivated(
            (disposable) =>
            {
                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.RecentlyUsedFiles,
                        (v) => v.RecentlyUsedFiles.ItemsSource
                    )
                    .DisposeWith(disposable);

                this.ViewModel
                    .WhenAnyValue((x) => x.RecentlyUsedFiles)
                    .Subscribe(
                        (ImmutableList<RecentlyUsedFileViewModel> list) =>
                        {
                            RecentlyUsedFiles.Visibility = list.IsEmpty
                                ? Visibility.Collapsed
                                : Visibility.Visible;
                            RecentlyUsedFilesEmpty.Visibility = list.IsEmpty
                                ? Visibility.Visible
                                : Visibility.Collapsed;
                        }
                    )
                    .DisposeWith(disposable);

                this.BindCommand(ViewModel, (vm) => vm.OpenFileCommand, (v) => v.OpenFile)
                    .DisposeWith(disposable);

                ViewModel!.PickCsvDocumentInteraction.RegisterHandler(PickCsvDocumentAsync);
            }
        );
    }

    private async Task PickCsvDocumentAsync(InteractionContext<Unit, string?> obj)
    {
        // Create a file picker
        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);

        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.List;
        openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        openPicker.FileTypeFilter.Add(".csv");

        // Open the picker for the user to pick a file
        var file = await openPicker.PickSingleFileAsync();

        if (file != null)
        {
            obj.SetOutput(file.Path);
        }
        else
        {
            obj.SetOutput(null);
        }
    }
}
