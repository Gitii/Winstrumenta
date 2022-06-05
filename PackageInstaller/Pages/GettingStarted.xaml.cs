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
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using PackageInstaller.Core.ModelViews;
using ReactiveUI;
using Splat;

namespace PackageInstaller.Pages;

public class ReactivePageGettingStarted : ReactivePage<GettingStartedModelView> { }

public sealed partial class GettingStarted
{
    public GettingStarted()
    {
        this.InitializeComponent();

        this.WhenActivated(
            (disposable) =>
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

                this.ViewModel!.PickFileInteraction
                    .RegisterHandler(PickFile)
                    .DisposeWith(disposable);
            }
        );
    }

    private async Task PickFile(InteractionContext<Unit, PickFileOutput> arg)
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
            && args.Data.GetView().AvailableFormats.Contains(StandardDataFormats.StorageItems);
    }

    private async Task<string?> GetDroppedFileAsync(DragEventArgs args)
    {
        var items = await args.DataView.GetStorageItemsAsync();

        return items.FirstOrDefault((i) => i.IsOfType(StorageItemTypes.File))?.Path;
    }
}
