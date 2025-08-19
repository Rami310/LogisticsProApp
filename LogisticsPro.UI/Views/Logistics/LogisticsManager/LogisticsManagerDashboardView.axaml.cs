using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Logistics.LogisticsManager
{
    public partial class LogisticsManagerDashboardView : BaseDashboardView
    {
        public LogisticsManagerDashboardView() : base("Logistics Manager Dashboard")
        {
            InitializeComponent();
    
            // IMPORTANT: Set up DataContext before the base class creates the UI
            this.Initialized += (sender, e) => {
                string username = this.Tag as string ?? "logistics_mgr";
        
                Console.WriteLine($"LogisticsManagerDashboardView initialized - Username: {username}");
        
                // Create view model first
                var viewModel = new LogisticsManagerDashboardViewModel(
                    () => App.GetNavigationService().NavigateTo("login"),
                    username
                );
        
                // Set DataContext
                this.DataContext = viewModel;
        
                Console.WriteLine($"Set LogisticsManagerDashboardView DataContext to: {viewModel.GetType().Name}");
            };
        }
        
        protected override UserControl CreateSidebar()
        {
            Console.WriteLine("Creating Logistics Sidebar");
            return new LogisticsManagerSidebarView();
        }
        
        protected override UserControl CreateContent()
        {
            Console.WriteLine("Creating Logistics Content");
            return new LogisticsManagerContentView();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}