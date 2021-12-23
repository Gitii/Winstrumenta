using System.Reactive.Disposables;
using PackageInstaller.Core.ModelViews;
using ReactiveUI;

namespace PackageInstaller.Pages
{
    public class ReactivePagePackageActions : ReactivePage<PackageActionsViewModel>
    {
    }

    public sealed partial class PackageActions
    {
        public PackageActions()
        {
            InitializeComponent();

            this.WhenActivated((disposable) =>
            {
                this.OneWayBind(ViewModel, (vm) => vm.PackageMetaData.Package, (v) => v.PackageName.Text).DisposeWith(disposable);
                this.OneWayBind(ViewModel, (vm) => vm.PackageMetaData.Architecture, (v) => v.Architecture.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, (vm) => vm.PackageMetaData.Version, (v) => v.Version.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, (vm) => vm.PackageMetaData.Description, (v) => v.Description.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel, (vm) => vm.DistroList, (v) => v.DistroList.ItemsSource)
                    .DisposeWith(disposable);

                this.Bind(ViewModel, (vm) => vm.SelectedWslDistribution, (v) => v.DistroList.SelectedValue)
                    .DisposeWith(disposable);
            });
        }
    }
}
