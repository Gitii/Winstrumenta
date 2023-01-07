using System;
using System.Linq;
using System.Reactive.Disposables;
using CommunityToolkit.WinUI.UI.Controls;
using DynamicData;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Numbers.Core.ModelViews;
using ReactiveUI;

namespace Numbers.Pages;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "MA0048:File name must match type name",
    Justification = "Workaround: Xaml doesn't support generic types."
)]
public class ReactivePageTable : ReactivePage<TableViewModel> { }

public sealed partial class Table
{
    public Table()
    {
        this.InitializeComponent();

        this.WhenActivated(
            (disposable) =>
            {
                this.ViewModel
                    .WhenAnyValue((vm) => vm.Columns)
                    .Subscribe(
                        (columns) =>
                        {
                            DataGrid.Columns.Clear();
                            DataGrid.Columns.AddRange(
                                columns.Select(
                                    (c) =>
                                        new DataGridTextColumn()
                                        {
                                            Binding = new Binding()
                                            {
                                                Path = new PropertyPath($"Cells.{c.fieldName}"),
                                                Mode = BindingMode.TwoWay
                                            },
                                            Header = c.header
                                        }
                                )
                            );
                        }
                    )
                    .DisposeWith(disposable);

                this.OneWayBind(ViewModel, (vm) => vm.Rows, (v) => v.DataGrid.ItemsSource)
                    .DisposeWith(disposable);
            }
        );
    }
}
