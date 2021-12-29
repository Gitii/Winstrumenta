using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Binding;
using Microsoft.UI.Xaml;
using PackageInstaller.Core.ModelViews;
using PackageInstaller.Core.Services;
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

            this.WhenActivated(
                (disposable) =>
                {
                    this.OneWayBind(
                            ViewModel,
                            (vm) => vm.PackageMetaData.Package,
                            (v) => v.PackageName.Text
                        )
                        .DisposeWith(disposable);

                    this.ViewModel
                        .WhenAnyValue((vm) => vm.InProgress, vm => vm.SelectedWslDistribution)
                        .CombineLatest(
                            ViewModel!.DistroList
                                .ToObservableChangeSet()
                                .Select((c) => ViewModel.DistroList.Count),
                            (firstT, distroListCount) =>
                                !firstT.Item1 && (firstT.Item2 == null || distroListCount > 1)
                        )
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .BindTo(this, v => v.DistroList.IsEnabled)
                        .DisposeWith(disposable);

                    this.OneWayBind(
                            ViewModel,
                            (vm) => vm.InProgress,
                            (v) => v.PrimaryActionButton.IsEnabled,
                            (isInProgress) => !isInProgress
                        )
                        .DisposeWith(disposable);

                    this.OneWayBind(
                            ViewModel,
                            (vm) => vm.PackageMetaData.Architecture,
                            (v) => v.Architecture.Text
                        )
                        .DisposeWith(disposable);
                    this.OneWayBind(
                            ViewModel,
                            (vm) => vm.PackageMetaData.Version,
                            (v) => v.Version.Text
                        )
                        .DisposeWith(disposable);
                    this.OneWayBind(
                            ViewModel,
                            (vm) => vm.PackageMetaData.Description,
                            (v) => v.Description.Text
                        )
                        .DisposeWith(disposable);

                    this.OneWayBind(
                            ViewModel,
                            (vm) => vm.DistroList,
                            (v) => v.DistroList.ItemsSource
                        )
                        .DisposeWith(disposable);

                    this.Bind(
                            ViewModel,
                            (vm) => vm.SelectedWslDistribution,
                            (v) => v.DistroList.SelectedItem
                        )
                        .DisposeWith(disposable);

                    this.OneWayBind(
                            ViewModel,
                            (vm) => vm.PackageInstallationStatus,
                            (v) => v.PrimaryActionButtonText.Text,
                            StatusToText
                        )
                        .DisposeWith(disposable);

                    ViewModel.PrimaryPackageCommand.IsExecuting
                        .Select(
                            (isExecuting) =>
                                isExecuting
                                    ? Microsoft.UI.Xaml.Visibility.Visible
                                    : Visibility.Collapsed
                        )
                        .BindTo(this, (v) => v.PrimaryActionButtonIcon.Visibility)
                        .DisposeWith(disposable);

                    this.ViewModel
                        .WhenAnyValue(
                            (vm) => vm.PackageInstallationStatus,
                            (vm) => vm.InstalledPackageVersion
                        )
                        .Select(StatusToRemarks)
                        .BindTo(this, (v) => v.Remarks.Text)
                        .DisposeWith(disposable);

                    this.BindCommand(
                            ViewModel,
                            (vm) => vm.PrimaryPackageCommand,
                            (v) => v.PrimaryActionButton
                        )
                        .DisposeWith(disposable);

                    ViewModel!.DistroList
                        .ToObservable()
                        .Subscribe(
                            item =>
                            {
                                if (ViewModel.SelectedWslDistribution != null)
                                {
                                    ViewModel.SelectedWslDistribution = item;
                                }
                            }
                        )
                        .DisposeWith(disposable);
                }
            );
        }

        private string StatusToRemarks(
            (IPlatformDependentPackageManager.PackageInstallationStatus? status, string? version) arg
        )
        {
            if (!arg.status.HasValue)
            {
                return String.Empty;
            }

            switch (arg.status)
            {
                case IPlatformDependentPackageManager.PackageInstallationStatus.NotInstalled:
                    return "Package isn't installed.";
                case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledSameVersion:
                    return "Same version is already installed.";
                case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledOlderVersion:
                    return $"An older version ({arg.version}) is installed";
                case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledNewerVersion:
                    return $"A newer version ({arg.version}) is installed";
                default:
                    return "Unknown status";
            }
        }

        private string StatusToText(IPlatformDependentPackageManager.PackageInstallationStatus? arg)
        {
            switch (arg)
            {
                case IPlatformDependentPackageManager.PackageInstallationStatus.NotInstalled:
                    return "Install";
                case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledSameVersion:
                    return "Reinstall";
                case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledOlderVersion:
                    return "Upgrade";
                case IPlatformDependentPackageManager.PackageInstallationStatus.InstalledNewerVersion:
                    return "Downgrade";
                case null:
                    return "Close";
                default:
                    throw new ArgumentOutOfRangeException(nameof(arg), arg, null);
            }
        }
    }
}