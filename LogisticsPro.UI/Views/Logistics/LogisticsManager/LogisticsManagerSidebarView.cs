using Avalonia.Controls;
using LogisticsPro.UI.ViewModels;
using LogisticsPro.UI.Views.Shared;

namespace LogisticsPro.UI.Views.Logistics.LogisticsManager
{
    public partial class LogisticsManagerSidebarView : BaseSidebarView
    {
        public LogisticsManagerSidebarView()
        {
            // Set Logistics Manager-specific role
            SetUserRole("Logistics Manager");
            
            // Create Logistics Manager-specific menu items
            SetupLogisticsManagerMenuItems();
        }

        private void SetupLogisticsManagerMenuItems()
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
                { Text = "Inventory Management", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            inventoryButton.Content = inventoryContent;

            // Shipments Button
            var shipmentsButton = new Button { Classes = { "sidebar-button" } };
            var shipmentsContent = new StackPanel
                { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            shipmentsContent.Children.Add(new TextBlock { Text = "🚚", FontSize = 16 });
            shipmentsContent.Children.Add(new TextBlock
                { Text = "Shipments", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            shipmentsButton.Content = shipmentsContent;

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
            menuPanel.Children.Add(shipmentsButton);
            menuPanel.Children.Add(reportsButton);

            // Set up bindings when DataContext is available
            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is ViewModels.LogisticsManagerDashboardViewModel viewModel)
                {
                    dashboardButton.Command = viewModel.NavigateToSectionCommand;
                    dashboardButton.CommandParameter = "Dashboard";

                    //Bind inventory button
                    inventoryButton.Command = viewModel.NavigateToSectionCommand;
                    inventoryButton.CommandParameter = "InventoryL";

                    shipmentsButton.Command = viewModel.NavigateToSectionCommand;
                    shipmentsButton.CommandParameter = "Shipments";

                    reportsButton.Command = viewModel.NavigateToSectionCommand;
                    reportsButton.CommandParameter = "Reports";
                }
            };

            SetMenuItems(menuPanel);
        }
    }
}