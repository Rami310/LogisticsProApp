using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Warehouse.WarehouseEmployee
{
    public partial class WarehouseEmployeeDashboardView : BaseDashboardView
    {
        public WarehouseEmployeeDashboardView() : base("Warehouse Employee Dashboard")
        {
            InitializeComponent();
    
            this.Initialized += (sender, e) => {
                string username = this.Tag as string ?? "user";

                Console.WriteLine($"WarehouseEmployeeDashboardView initialized - Username: {username}");

                // Create view model
                var viewModel = new ViewModels.WarehouseEmployeeDashboardViewModel(
                    () => App.GetNavigationService().NavigateTo("login"),
                    username
                );

                // Set DataContext
                this.DataContext = viewModel;
        
                Console.WriteLine($"Set WarehouseEmployeeDashboardView DataContext to: {viewModel.GetType().Name}");
            };
        }
        
        protected override UserControl CreateSidebar()
        {
            return new WarehouseEmployeeSidebarView();
        }
        
        protected override UserControl CreateContent()
        {
            return new WarehouseEmployeeContentView();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}