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
            var dashboardContent = new StackPanel
                { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            dashboardContent.Children.Add(new TextBlock { Text = "🏠", FontSize = 16 });
            dashboardContent.Children.Add(new TextBlock
                { Text = "Dashboard", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            dashboardButton.Content = dashboardContent;

            // Reports Button  
            var reportsButton = new Button { Classes = { "sidebar-button" } };
            var reportsContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            reportsContent.Children.Add(new TextBlock { Text = "📊", FontSize = 16 });
            reportsContent.Children.Add(new TextBlock
                { Text = "Reports", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            reportsButton.Content = reportsContent;

            // Language Toggle Button
            var languageButton = new Button { Classes = { "sidebar-button" } };
            var languageContent = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };
            languageContent.Children.Add(new TextBlock { Text = "🌐", FontSize = 16 });

            var languageText = new TextBlock
                { Text = "Language", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            languageContent.Children.Add(languageText);
            languageButton.Content = languageContent;


            // buttons to menu panel
            menuPanel.Children.Add(dashboardButton);
            menuPanel.Children.Add(reportsButton);
            menuPanel.Children.Add(languageButton);

            // Set up bindings when DataContext is available
            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is ViewModels.AdminDashboardViewModel viewModel)
                {
                    dashboardButton.Command = viewModel.NavigateToSectionCommand;
                    dashboardButton.CommandParameter = "Dashboard";

                    reportsButton.Command = viewModel.NavigateToSectionCommand;
                    reportsButton.CommandParameter = "Reports";

                    languageButton.Command = viewModel.ToggleLanguageCommand;
                    // Update button text when language changes
                    viewModel.PropertyChanged += (s2, e2) =>
                    {
                        if (e2.PropertyName == nameof(viewModel.LanguageText))
                        {
                            languageText.Text = viewModel.LanguageText;
                        }
                    };

                    SetMenuItems(menuPanel);
                }
            };
        }
    }
}