using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LogisticsPro.UI.Views.Logistics.LogisticsManager.Sections;

public partial class ReportsSection : UserControl
{
    public ReportsSection()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}