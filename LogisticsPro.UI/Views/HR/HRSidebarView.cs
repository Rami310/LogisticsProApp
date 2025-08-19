using Avalonia.Controls;
using LogisticsPro.UI.Views.Shared;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.HR
{
    public partial class HRSidebarView : BaseSidebarView
    {
        public HRSidebarView()
        {
            // Set HR-specific role
            SetUserRole("HR Manager");
            
            // Create HR-specific menu items
            SetupHRMenuItems();
        }

        private void SetupHRMenuItems()
        {
            var menuPanel = new StackPanel { Spacing = 15 };

            // Employees Button
            var employeesButton = new Button { Classes = { "sidebar-button" } };
            var employeesContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            employeesContent.Children.Add(new TextBlock { Text = "👥", FontSize = 16 });
            employeesContent.Children.Add(new TextBlock { Text = "Employees", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            employeesButton.Content = employeesContent;
            
            // Departments Button
            var departmentsButton = new Button { Classes = { "sidebar-button" } };
            var departmentsContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            departmentsContent.Children.Add(new TextBlock { Text = "📋", FontSize = 16 });
            departmentsContent.Children.Add(new TextBlock { Text = "Departments", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            departmentsButton.Content = departmentsContent;

            // HR Reports Button
            var reportsButton = new Button { Classes = { "sidebar-button" } };
            var reportsContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            reportsContent.Children.Add(new TextBlock { Text = "📊", FontSize = 16 });
            reportsContent.Children.Add(new TextBlock { Text = "HR Reports", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            reportsButton.Content = reportsContent;

            // Add buttons to menu panel
            menuPanel.Children.Add(employeesButton);
            menuPanel.Children.Add(departmentsButton);
            menuPanel.Children.Add(reportsButton);

            // Set up bindings when DataContext is available
            this.DataContextChanged += (s, e) => {
                if (DataContext is HRDashboardViewModel viewModel)
                {
                    employeesButton.Command = viewModel.NavigateToSectionCommand;
                    employeesButton.CommandParameter = "Employees";
                    
                    departmentsButton.Command = viewModel.NavigateToSectionCommand;
                    departmentsButton.CommandParameter = "Departments";
                }
            };

            SetMenuItems(menuPanel);
        }

    }
}