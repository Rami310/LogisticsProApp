using Avalonia.Controls;
using LogisticsPro.UI.ViewModels;
using LogisticsPro.UI.Views.Shared;

namespace LogisticsPro.UI.Views.Logistics.LogisticsEmployee
{
    public partial class LogisticsEmployeeSidebarView : BaseSidebarView
    {
        public LogisticsEmployeeSidebarView()
        {
            // Set Logistics Employee-specific role
            SetUserRole("Logistics Employee");
            
            // Create Logistics Employee-specific menu items
            SetupLogisticsEmployeeMenuItems();
        }

        private void SetupLogisticsEmployeeMenuItems()
        {
            var menuPanel = new StackPanel { Spacing = 15 };

            // 1. Dashboard Button (Main Overview)
            var dashboardButton = new Button { Classes = { "sidebar-button" } };
            var dashboardContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            dashboardContent.Children.Add(new TextBlock { Text = "🏠", FontSize = 16 });
            dashboardContent.Children.Add(new TextBlock { Text = "Dashboard", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            dashboardButton.Content = dashboardContent;

            // 2. My Tasks Button (Delivery Tasks Section)
            var tasksButton = new Button { Classes = { "sidebar-button" } };
            var tasksContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            tasksContent.Children.Add(new TextBlock { Text = "🚚", FontSize = 16 });
            tasksContent.Children.Add(new TextBlock { Text = "My Tasks", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            tasksButton.Content = tasksContent;

            // 3. Performance Button (Future Section)
            var performanceButton = new Button { Classes = { "sidebar-button" } };
            var performanceContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            performanceContent.Children.Add(new TextBlock { Text = "📊", FontSize = 16 });
            performanceContent.Children.Add(new TextBlock { Text = "Performance", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            performanceButton.Content = performanceContent;

            // Add buttons to menu panel in order
            menuPanel.Children.Add(dashboardButton);
            menuPanel.Children.Add(tasksButton);
            menuPanel.Children.Add(performanceButton);

            // Set up bindings when DataContext is available
            this.DataContextChanged += (s, e) => {
                if (DataContext is ViewModels.LogisticsEmployeeDashboardViewModel viewModel)
                {
                    dashboardButton.Command = viewModel.NavigateToSectionCommand;
                    dashboardButton.CommandParameter = "Dashboard";
                    
                    tasksButton.Command = viewModel.NavigateToSectionCommand;
                    tasksButton.CommandParameter = "MyTasks";
                    
                    performanceButton.Command = viewModel.NavigateToSectionCommand;
                    performanceButton.CommandParameter = "Performance";
                }
            };

            SetMenuItems(menuPanel);
        }
    }
}