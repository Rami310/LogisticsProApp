using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LogisticsPro.UI.Views.Warehouse.WarehouseManager.Sections;

public partial class DashboardOverviewSection : UserControl
{
    public DashboardOverviewSection()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}