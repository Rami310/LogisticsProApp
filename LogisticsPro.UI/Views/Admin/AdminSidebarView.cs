using Avalonia.Controls;
using LogisticsPro.UI.Views.Shared;

namespace LogisticsPro.UI.Views.Admin
{
    public partial class AdminSidebarView : BaseSidebarView
    {
        public AdminSidebarView()
        {
            SetUserRole("Administrator");
            SetupAdminMenuItems();
        }

        private void SetupAdminMenuItems()
        {
            var menuPanel = new StackPanel { Spacing = 15 };

            // Dashboard Button
            var dashboardButton = new Button { Classes = { "sidebar-button" } };
            var dashboardContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            dashboardContent.Children.Add(new TextBlock { Text = "🏠", FontSize = 16 });
            dashboardContent.Children.Add(new TextBlock { Text = "Dashboard", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            dashboardButton.Content = dashboardContent;
            
            // Reports Button  
            var reportsButton = new Button { Classes = { "sidebar-button" } };
            var reportsContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            reportsContent.Children.Add(new TextBlock { Text = "📊", FontSize = 16 });
            reportsContent.Children.Add(new TextBlock { Text = "Reports", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            reportsButton.Content = reportsContent;
            
            // Add buttons to menu panel (removed Inventory and Shipments)
            menuPanel.Children.Add(dashboardButton);
            menuPanel.Children.Add(reportsButton);

            // Set up bindings when DataContext is available
            this.DataContextChanged += (s, e) => {
                if (DataContext is ViewModels.AdminDashboardViewModel viewModel)
                {
                    dashboardButton.Command = viewModel.NavigateToSectionCommand;
                    dashboardButton.CommandParameter = "Dashboard";
                    
                    reportsButton.Command = viewModel.NavigateToSectionCommand;
                    reportsButton.CommandParameter = "Reports";
                }
            };

            SetMenuItems(menuPanel);
        }
    }
}