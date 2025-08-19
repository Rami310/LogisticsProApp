using Avalonia.Controls;
using LogisticsPro.UI.ViewModels;
using LogisticsPro.UI.Views.Shared;

namespace LogisticsPro.UI.Views.Warehouse.WarehouseEmployee
{
    public partial class WarehouseEmployeeSidebarView : BaseSidebarView
    {
        public WarehouseEmployeeSidebarView()
        {
            // Set Warehouse Employee-specific role
            SetUserRole("Warehouse Employee");
            
            // Create Warehouse Employee-specific menu items
            SetupWarehouseEmployeeMenuItems();
        }

        private void SetupWarehouseEmployeeMenuItems()
        {
            var menuPanel = new StackPanel { Spacing = 15 };

            // Dashboard Button
            var dashboardButton = new Button { Classes = { "sidebar-button" } };
            var dashboardContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            dashboardContent.Children.Add(new TextBlock { Text = "🏠", FontSize = 16 });
            dashboardContent.Children.Add(new TextBlock { Text = "Dashboard", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            dashboardButton.Content = dashboardContent;
            
            // Receiving Button
            var receivingButton = new Button { Classes = { "sidebar-button" } };
            var receivingContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            receivingContent.Children.Add(new TextBlock { Text = "📦", FontSize = 16 });
            receivingContent.Children.Add(new TextBlock { Text = "Receiving", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            receivingButton.Content = receivingContent;

            // Add buttons to menu panel
            menuPanel.Children.Add(dashboardButton);
            menuPanel.Children.Add(receivingButton);

            // Set up bindings when DataContext is available
            this.DataContextChanged += (s, e) => {
                if (DataContext is ViewModels.WarehouseEmployeeDashboardViewModel viewModel)
                {
                    dashboardButton.Command = viewModel.NavigateToSectionCommand;
                    dashboardButton.CommandParameter = "Dashboard";
                    
                    receivingButton.Command = viewModel.NavigateToSectionCommand;
                    receivingButton.CommandParameter = "Receiving";
                }
            };

            SetMenuItems(menuPanel);
        }
    }
}