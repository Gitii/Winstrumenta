using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PackageInstaller.Core.ModelViews;
using ReactiveUI;

namespace PackageInstaller.Pages;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "MA0048:File name must match type name",
    Justification = "Workaround: Xaml doesn't support generic types."
)]
public class ReactivePageActionExecution : ReactivePage<ActionExecutionViewModel>
{
}

public sealed partial class ActionExecution
{
    public ActionExecution()
    {
        this.InitializeComponent();

        this.WhenActivated(
            (disposable) =>
            {
                this.OneWayBind(ViewModel, (vm) => vm.PackageLabel, (v) => v.PackageLabel.Text)
                    .DisposeWith(disposable);

                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.OperationList,
                        (v) => v.OperationList.ItemsSource
                    )
                    .DisposeWith(disposable);

                this.OneWayBind(ViewModel, (vm) => vm.ActionFailed, (v) => v.ErrorActionPanel.Visibility,
                        hasFailed => hasFailed ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposable);

                this.BindCommand(ViewModel, (vm) => vm.CloseCommand, (v) => v.CloseButton).DisposeWith(disposable);

                this.BindCommand(ViewModel, (vm) => vm.RetryActionCommand, (v) => v.RetryButton)
                    .DisposeWith(disposable);

                this.BindCommand(ViewModel, (vm) => vm.ShowErrorActions, (v) => v.ShowErrorDetailsButton)
                    .DisposeWith(disposable);

                this.ViewModel!.ShowPopupInteraction.RegisterHandler(ShowPopupAsync).DisposeWith(disposable);
            }
        );
    }

    private Task ShowPopupAsync(InteractionContext<PopupInput, Unit> interaction)
    {
        var dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = interaction.Input.Title,
            Content = new ScrollViewer()
            {
                Content = new TextBlock()
                {
                    Text = interaction.Input.Content,
                    FontFamily = new FontFamily("Courier New"),
                    TextWrapping = TextWrapping.NoWrap,
                    FontSize = 10,
                },
                VerticalScrollMode = ScrollMode.Auto,
                HorizontalScrollMode = ScrollMode.Disabled
            },
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "Close",
        };

        return dialog.ShowAsync().AsTask();
    }
}
