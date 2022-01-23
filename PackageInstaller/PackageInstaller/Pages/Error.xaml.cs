using System.Reactive.Disposables;
using Microsoft.UI.Xaml;
using PackageInstaller.Core.ModelViews;
using PackageInstaller.Core.Services;
using ReactiveUI;
using Splat;

namespace PackageInstaller.Pages;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name",
    Justification = "Workaround: Xaml doesn't support generic types.")]
public class ReactivePageError : ReactivePage<ErrorViewModel>
{
}

public sealed partial class Error
{
    public Error()
    {
        this.InitializeComponent();

        this.WhenActivated(
            (disposable) =>
            {
                this.OneWayBind(ViewModel, (vm) => vm.ErrorTitle, (v) => v.Title.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.ErrorDescription,
                        (v) => v.Description.Text
                    )
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, (vm) => vm.ErrorDetails, (v) => v.ErrorDetails.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.ErrorDetailsVisible,
                        (v) => v.ErrorDetails.Visibility,
                        (bool isVisible) =>
                            isVisible ? Visibility.Visible : Visibility.Collapsed
                    )
                    .DisposeWith(disposable);
                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.ErrorDetailsVisible,
                        (v) => v.ToggleErrorDetails.Content,
                        (bool isVisible) => isVisible ? "Hide details" : "Show details"
                    )
                    .DisposeWith(disposable);
                this.BindCommand(ViewModel, (vm) => vm.Close, (v) => v.Button)
                    .DisposeWith(disposable);
                this.BindCommand(
                        ViewModel,
                        (vm) => vm.ToggleErrorDetails,
                        (v) => v.ToggleErrorDetails
                    )
                    .DisposeWith(disposable);
            }
        );

        Locator.Current.GetService<IThemeManager>()!.SetTitleBarColor(255, 139, 0, 0);
    }
}
