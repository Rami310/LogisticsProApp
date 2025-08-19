using Avalonia.Controls;
using LogisticsPro.UI.ViewModels;
using LogisticsPro.UI.Views.Shared;

namespace LogisticsPro.UI.Views.Warehouse.WarehouseManager
{
    public partial class WarehouseManagerSidebarView : BaseSidebarView
    {
        public WarehouseManagerSidebarView()
        {
            // Set Warehouse Manager-specific role
            SetUserRole("Warehouse Manager");
            
            // Create Warehouse Manager-specific menu items
            SetupWarehouseManagerMenuItems();
        }

        private void SetupWarehouseManagerMenuItems()
        {
            var menuPanel = new StackPanel { Spacing = 15 };

            // Dashboard Button
            var dashboardButton = new Button { Classes = { "sidebar-button" } };
            var dashboardContent = new StackPanel
                { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            dashboardContent.Children.Add(new TextBlock { Text = "🏠", FontSize = 16 });
            dashboardContent.Children.Add(new TextBlock
                { Text = "Dashboard", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            dashboardButton.Content = dashboardContent;

            // Inventory Button
            var inventoryButton = new Button { Classes = { "sidebar-button" } };
            var inventoryContent = new StackPanel
                { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            inventoryContent.Children.Add(new TextBlock { Text = "📦", FontSize = 16 });
            inventoryContent.Children.Add(new TextBlock
                { Text = "Inventory Overview", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            inventoryButton.Content = inventoryContent;

            // Product Requests Button
            var requestsButton = new Button { Classes = { "sidebar-button" } };
            var requestsContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            requestsContent.Children.Add(new TextBlock { Text = "📋", FontSize = 16 });
            requestsContent.Children.Add(new TextBlock
                { Text = "Product Requests", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            requestsButton.Content = requestsContent;

            // Reports Button
            var reportsButton = new Button { Classes = { "sidebar-button" } };
            var reportsContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            reportsContent.Children.Add(new TextBlock { Text = "📊", FontSize = 16 });
            reportsContent.Children.Add(new TextBlock
                { Text = "Reports", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            reportsButton.Content = reportsContent;

            // Add buttons to menu panel
            menuPanel.Children.Add(dashboardButton);
            menuPanel.Children.Add(inventoryButton);
            menuPanel.Children.Add(requestsButton);
            menuPanel.Children.Add(reportsButton);

            // Set up bindings with correct parameter names
            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is WarehouseManagerDashboardViewModel viewModel)
                {
                    dashboardButton.Command = viewModel.NavigateToSectionCommand;
                    dashboardButton.CommandParameter = "Dashboard";

                    inventoryButton.Command = viewModel.NavigateToSectionCommand;
                    inventoryButton.CommandParameter = "InventoryCheck";

                    requestsButton.Command = viewModel.NavigateToSectionCommand;
                    requestsButton.CommandParameter = "ProductRequests";

                    reportsButton.Command = viewModel.NavigateToSectionCommand;
                    reportsButton.CommandParameter = "Reports";
                }
            };

            SetMenuItems(menuPanel);
        }
    }
}