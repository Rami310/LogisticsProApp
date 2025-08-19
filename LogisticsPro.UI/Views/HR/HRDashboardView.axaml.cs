using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.ViewModels;
using LogisticsPro.UI.Views.HR;

namespace LogisticsPro.UI.Views.HR
{
    public partial class HRDashboardView : BaseDashboardView
    {
        public HRDashboardView() : base("HR Dashboard")
        {
            InitializeComponent();
    
            // Set up DataContext before the base class creates the UI
            this.Initialized += (sender, e) => {
                string username = this.Tag as string ?? "user";
        
                Console.WriteLine($"HRDashboardView initialized - Username: {username}");
        
                // Create view model first
                var viewModel = new HRDashboardViewModel(
                    () => App.GetNavigationService().NavigateTo("login"),
                    username
                );
        
                // Set DataContext
                this.DataContext = viewModel;
        
                Console.WriteLine($"Set HRDashboardView DataContext to: {viewModel.GetType().Name}");
            };
        }
        
        protected override UserControl CreateSidebar()
        {
            Console.WriteLine("Creating HR Sidebar");
            return new HRSidebarView();
        }
        
        protected override UserControl CreateContent()
        {
            Console.WriteLine("Creating HR Content");
            return new HRContentView();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}