using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LogisticsPro.UI.Views.Logistics.LogisticsManager.Sections;

public partial class InventoryLSection : UserControl
{
    public InventoryLSection()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}