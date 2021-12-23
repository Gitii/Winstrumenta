using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PackageInstaller.Controls
{
    public sealed partial class PlaceholderContainer : UserControl
    {
        public PlaceholderContainer()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
            "Rows",
            typeof(int),
            typeof(PlaceholderContainer),
            new PropertyMetadata(0, OnRowsChanged)
        );

        private static void OnRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PlaceholderContainer container)
            {
                container.Items.ItemsSource = Enumerable.Range(0, (int)e.NewValue).ToList();
            }
        }

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }
    }
}
