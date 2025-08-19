using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LogisticsPro.UI.Views.Warehouse.WarehouseEmployee
{
    public partial class WarehouseEmployeeContentView : UserControl
    {
        public WarehouseEmployeeContentView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}