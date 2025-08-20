using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using static Avalonia.Controls.DataGrid;


namespace LogisticsPro.UI.Views.Warehouse.WarehouseManager.Sections;

public partial class InventoryManagementSection : UserControl
{
    public InventoryManagementSection()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}