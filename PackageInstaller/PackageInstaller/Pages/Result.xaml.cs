using System;
using System.Reactive.Disposables;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using PackageInstaller.AttachedProperties;
using PackageInstaller.Core.ModelViews;
using ReactiveUI;

namespace PackageInstaller.Pages;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "MA0048:File name must match type name",
    Justification = "Workaround: Xaml doesn't support generic types."
)]
public class ReactivePageResult : ReactivePage<ResultViewModel>
{
}

public sealed partial class Result
{
    public Result()
    {
        this.InitializeComponent();

        this.WhenActivated(
            (disposable) =>
            {
                this.OneWayBind(ViewModel, (vm) => vm.Title, (v) => v.Title.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, (vm) => vm.Description, (v) => v.Description.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, (vm) => vm.Details, (v) => v.Details.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.AreDetailsVisible,
                        (v) => v.DetailsContainer.Visibility,
                        (bool isVisible) =>
                            isVisible ? Visibility.Visible : Visibility.Collapsed
                    )
                    .DisposeWith(disposable);
                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.AreDetailsVisible,
                        (v) => v.ToggleDetails.Content,
                        (bool isVisible) => isVisible ? "Hide details" : "Show details"
                    )
                    .DisposeWith(disposable);
                this.OneWayBind(
                    ViewModel,
                    (vm) => vm.Theme,
                    (v) => v.Icon.Source,
                    GetBitmapSourceFromTheme
                );

                this.BindCommand(ViewModel, (vm) => vm.CloseCommand, (v) => v.Button)
                    .DisposeWith(disposable);
                this.BindCommand(
                        ViewModel,
                        (vm) => vm.ToggleDetailsCommand,
                        (v) => v.ToggleDetails
                    )
                    .DisposeWith(disposable);
            }
        );
    }

    private ImageSource GetBitmapSourceFromTheme(ResultViewModel.ResultTheme theme)
    {
        switch (theme)
        {
            case ResultViewModel.ResultTheme.Success:
                return ImageExtensions.GetImageSourceFromUri("Assets/success.png")!;
            case ResultViewModel.ResultTheme.Warning:
                return ImageExtensions.GetImageSourceFromUri("Assets/warning.png")!;
            case ResultViewModel.ResultTheme.Error:
                return ImageExtensions.GetImageSourceFromUri("Assets/error.png")!;
            default:
                throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
        }
    }
}
