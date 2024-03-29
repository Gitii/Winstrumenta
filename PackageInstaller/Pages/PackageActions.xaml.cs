﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using PackageInstaller.Core.ModelViews;
using PackageInstaller.Core.Services;
using ReactiveUI;
using Windows.Foundation;

namespace PackageInstaller.Pages;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "MA0048:File name must match type name",
    Justification = "Workaround: Xaml doesn't support generic types."
)]
public class ReactivePagePackageActions : ReactivePage<PackageActionsViewModel> { }

public sealed partial class PackageActions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "MA0051:Method is too long",
        Justification = "<Pending>"
    )]
    public PackageActions()
    {
        InitializeComponent();

        this.WhenActivated(
            (disposable) =>
            {
                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.PackageMetaData.PackageLabel,
                        (v) => v.PackageLabel.Text
                    )
                    .DisposeWith(disposable);
                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.PackageMetaData.PackageName,
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
                        (v) => v.ActionButton.IsEnabled,
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
                        (vm) => vm.PackageMetaData.VersionLabel,
                        (v) => v.Version.Text
                    )
                    .DisposeWith(disposable);
                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.PackageMetaData.Description,
                        (v) => v.Description.Text
                    )
                    .DisposeWith(disposable);

                this.OneWayBind(ViewModel, (vm) => vm.DistroList, (v) => v.DistroList.ItemsSource)
                    .DisposeWith(disposable);

                this.Bind(
                        ViewModel,
                        (vm) => vm.SelectedWslDistribution,
                        (v) => v.DistroList.SelectedItem
                    )
                    .DisposeWith(disposable);

                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.PrimaryAction!.Title,
                        (v) => v.PrimaryActionButtonText.Text
                    )
                    .DisposeWith(disposable);

                this.ViewModel
                    .WhenAnyValue((vm) => vm.PrimaryAction!.ToolTip)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(
                        (tooltip) => ToolTipService.SetToolTip(PrimaryActionButtonText, tooltip)
                    )
                    .DisposeWith(disposable);

                this.ViewModel
                    .WhenAnyValue((vm) => vm.PackageIconStream)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .BindTo(this, (v) => v.PackageIcon.Source)
                    .DisposeWith(disposable);

                this.ViewModel
                    .WhenAnyObservable((vm) => vm.PrimaryAction!.ActionCommand.IsExecuting)
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
                        (vm) => vm.InProgress,
                        (vm) => vm.PackageInstallationStatus,
                        (vm) => vm.InstalledPackageVersion
                    )
                    .Select(StatusToRemarks)
                    .BindTo(this, (v) => v.Remarks.Text)
                    .DisposeWith(disposable);

                this.BindCommand(
                        ViewModel,
                        (vm) => vm.PrimaryAction!.ActionCommand,
                        (v) => v.ActionButton,
                        (vm) => vm.PrimaryAction
                    )
                    .DisposeWith(disposable);

                this.OneWayBind(
                        ViewModel,
                        (vm) => vm.PrimaryAction!.Title,
                        (v) => v.PrimaryActionButtonText.Text
                    )
                    .DisposeWith(disposable);

                this.ViewModel
                    .WhenAnyValue((vm) => vm.SecondaryActions)
                    .Subscribe(
                        (list) =>
                        {
                            SecondaryActions.Items.Clear();
                            SecondaryActions.Items.AddRange(
                                list.Select(
                                    (a) =>
                                    {
                                        var item = new MenuFlyoutItem()
                                        {
                                            Text = a.Title,
                                            Command = a.ActionCommand,
                                            CommandParameter = a
                                        };
                                        ToolTipService.SetToolTip(item, a.ToolTip);
                                        return item;
                                    }
                                )
                            );
                        }
                    )
                    .DisposeWith(disposable);

                this.ViewModel.DistroList
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

                this.OneWayBind(
                        ViewModel,
                        (mv) => mv.Notifications,
                        (mv) => mv.NotificationIconGlyph.Visibility,
                        (list) => list.Count == 0 ? Visibility.Collapsed : Visibility.Visible
                    )
                    .DisposeWith(disposable);

                this.OneWayBind(
                        ViewModel,
                        (mv) => mv.NotificationIconType,
                        (mv) => mv.NotificationIconGlyph.Priority
                    )
                    .DisposeWith(disposable);

                Observable
                    .FromEventPattern<PointerEventHandler, PointerRoutedEventArgs>(
                        ev => this.NotificationIconGlyph.PointerReleased += ev,
                        ev => this.NotificationIconGlyph.PointerReleased -= ev
                    )
                    .Select(x => Unit.Default)
                    .InvokeCommand(ViewModel!.GotoNotificationHub)
                    .DisposeWith(disposable);

                this.ViewModel.ActionConfirmationDialogInteraction.RegisterHandler(
                    (interaction) =>
                    {
                        TaskCompletionSource src = new TaskCompletionSource();
                        RoutedEventHandler handleClick = null!;
                        TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs>? handleClose =
                            null!;

                        handleClick = (_, _) =>
                        {
                            ActionFlyout.Closing -= handleClose;
                            ActionFlyoutButton.Click -= handleClick;

                            if (!interaction.IsHandled)
                            {
                                interaction.SetOutput(true);
                            }

                            src.SetResult();
                        };

                        handleClose = (_, _) =>
                        {
                            ActionFlyout.Closing -= handleClose;
                            ActionFlyoutButton.Click -= handleClick;

                            if (!interaction.IsHandled)
                            {
                                interaction.SetOutput(false);
                            }

                            src.SetResult();
                        };
                        ActionFlyout.Closing += handleClose;
                        ActionFlyoutButton.Click += handleClick;
                        ActionFlyout.ShowAt(ActionButton);
                        ActionFlyoutText.Text = interaction.Input;

                        return src.Task;
                    }
                );
            }
        );
    }

    private string StatusToRemarks(
        (bool isInProgress, IPlatformDependentPackageManager.PackageInstallationStatus? status, string? version) arg
    )
    {
        if (arg.isInProgress)
        {
            return "Loading...";
        }

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
}
