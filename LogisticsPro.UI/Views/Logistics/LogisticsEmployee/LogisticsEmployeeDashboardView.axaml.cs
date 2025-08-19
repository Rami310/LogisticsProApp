using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Logistics.LogisticsEmployee
{
    public partial class LogisticsEmployeeDashboardView : BaseDashboardView
    {
        public LogisticsEmployeeDashboardView() : base("Logistics Employee Dashboard")
        {
            InitializeComponent();
    
            // IMPORTANT: Set up DataContext before the base class creates the UI
            this.Initialized += (sender, e) => {
                string username = this.Tag as string ?? "log_emp1";
        
                Console.WriteLine($"LogisticsEmployeeDashboardView initialized - Username: {username}");
        
                // Create view model first
                var viewModel = new LogisticsEmployeeDashboardViewModel(
                    () => App.GetNavigationService().NavigateTo("login"),
                    username
                );
        
                // Set DataContext
                this.DataContext = viewModel;
        
                Console.WriteLine($"Set LogisticsEmployeeDashboardView DataContext to: {viewModel.GetType().Name}");
            };
        }
        
        protected override UserControl CreateSidebar()
        {
            Console.WriteLine("Creating Logistics Employee Sidebar");
            return new LogisticsEmployee.LogisticsEmployeeSidebarView();
        }
        
        protected override UserControl CreateContent()
        {
            Console.WriteLine("Creating Logistics Employee Content");
            return new LogisticsEmployee.LogisticsEmployeeContentView();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}