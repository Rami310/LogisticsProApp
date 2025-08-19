using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Warehouse.WarehouseManager
{
    public partial class WarehouseManagerDashboardView : BaseDashboardView
    {
        public WarehouseManagerDashboardView() : base("Warehouse Manager Dashboard")
        {
            InitializeComponent();
            
            // Set up DataContext before the base class creates the UI
            this.Initialized += (sender, e) => {
                string username = this.Tag as string ?? "user";
                
                Console.WriteLine($"WarehouseManagerDashboardView initialized - Username: {username}");
                
                // Create view model first
                var viewModel = new WarehouseManagerDashboardViewModel(
                    () => App.GetNavigationService().NavigateTo("login"),
                    username
                );
                
                // Set DataContext
                this.DataContext = viewModel;
                
                Console.WriteLine($"Set WarehouseManagerDashboardView DataContext to: {viewModel.GetType().Name}");
            };
        }
        
        protected override UserControl CreateSidebar()
        {
            Console.WriteLine("Creating Warehouse Manager Sidebar");
            return new WarehouseManagerSidebarView();
        }
        
        protected override UserControl CreateContent()
        {
            Console.WriteLine("Creating Warehouse Manager Content");
            return new WarehouseManagerContentView();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}