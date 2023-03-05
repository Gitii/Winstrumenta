using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using PackageInstaller.Core.ModelViews;
using ReactiveUI;
using Splat;
using Brush = Microsoft.UI.Xaml.Media.Brush;

namespace PackageInstaller.Pages;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "MA0048:File name must match type name",
    Justification = "Workaround: Xaml doesn't support generic types."
)]
public class ReactivePageGettingStarted : ReactivePage<GettingStartedModelView>
{
}

public sealed partial class GettingStarted
{
    public GettingStarted()
    {
        this.InitializeComponent();

        this.WhenActivated(
            (disposable) =>
            {
                SetupSectionHandlers(disposable);

                this.ViewModel!.PickFileInteraction
                    .RegisterHandler(PickFileAsync)
                    .DisposeWith(disposable);

                this.BindCommand(ViewModel, (vm) => vm.ExitCommand, (v) => v.ExitButton).DisposeWith(disposable);
            }
        );
    }

    private void SetupSectionHandlers(CompositeDisposable disposable)
    {
        Observable
            .FromEventPattern<PointerEventHandler, PointerRoutedEventArgs>(
                ev => this.PickSection.PointerPressed += ev,
                ev => this.PickSection.PointerPressed -= ev
            )
            .Where(x => !x.EventArgs.Handled)
            .Select((x) => Unit.Default)
            .InvokeCommand(ViewModel!.PickFilesCommand)
            .DisposeWith(disposable);

        Observable
            .FromEventPattern<
                TypedEventHandler<Hyperlink, HyperlinkClickEventArgs>,
                HyperlinkClickEventArgs
            >(ev => this.PickHyperLink.Click += ev, ev => this.PickHyperLink.Click -= ev)
            .Select(x => Unit.Default)
            .InvokeCommand(ViewModel!.PickFilesCommand)
            .DisposeWith(disposable);

        Observable
            .FromEventPattern<PointerEventHandler, PointerRoutedEventArgs>(
                ev => this.ExplorerSection.PointerPressed += ev,
                ev => this.ExplorerSection.PointerPressed -= ev
            )
            .Select(x => x.EventArgs)
            .Where(x => !x.Handled)
            .Select((x) => Unit.Default)
            .InvokeCommand(ViewModel!.LaunchExplorerCommand)
            .DisposeWith(disposable);

        Observable
            .FromEventPattern<DragEventHandler, DragEventArgs>(
                ev => this.PickSection.Drop += ev,
                ev => this.PickSection.Drop -= ev
            )
            .Select(x => x.EventArgs)
            .Where(IsValidDrop)
            .SelectMany(GetDroppedFileAsync)
            .InvokeCommand(ViewModel!.LaunchWithFileCommand)
            .DisposeWith(disposable);

        Observable
            .FromEventPattern<DragEventHandler, DragEventArgs>(
                ev => this.PickSection.DragOver += ev,
                ev => this.PickSection.DragOver -= ev
            )
            .Select(x => x.EventArgs)
            .Subscribe(
                (args) =>
                {
                    args.AcceptedOperation = IsValidDrop(args)
                        ? DataPackageOperation.Copy
                        : DataPackageOperation.None;
                }
            )
            .DisposeWith(disposable);
    }

    private async Task PickFileAsync(InteractionContext<Unit, PickFileOutput> arg)
    {
        var window = Locator.Current.GetService<MainWindow>()!;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var folderPicker = new FileOpenPicker();
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
        folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
        folderPicker.FileTypeFilter.Add("*");
        var result = await folderPicker.PickSingleFileAsync();

        if (result?.IsAvailable ?? false)
        {
            arg.SetOutput(new PickFileOutput() { PickedFilePath = result.Path });
        }
        else
        {
            arg.SetOutput(new PickFileOutput() { PickedFilePath = null });
        }
    }

    private bool IsValidDrop(DragEventArgs args)
    {
        return !args.Handled
               && args.DataView.Contains(StandardDataFormats.StorageItems);
    }

    private async Task<string?> GetDroppedFileAsync(DragEventArgs args)
    {
        var items = await args.DataView.GetStorageItemsAsync();

        return items.FirstOrDefault((i) => i.IsOfType(StorageItemTypes.File))?.Path;
    }

    private void Section_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        var control = (Border)sender;
        control.Background = (Brush)Resources["ShiningTranslucentGray"];
    }

    private void Section_OnPointerLeft(object sender, PointerRoutedEventArgs e)
    {
        var control = (Border)sender;
        control.Background = (Brush)Resources["TranslucentGray"];
    }
}
