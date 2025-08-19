using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Admin
{
    public partial class AdminDashboardView : BaseDashboardView
    {
        public AdminDashboardView() : base("Admin Dashboard")
        {
            InitializeComponent();
            
            // Set up when attached to visual tree
            this.Initialized += (sender, e) => {  // Changed from AttachedToVisualTree
                string username = this.Tag as string ?? "user";
    
                // Create view model
                var viewModel = new AdminDashboardViewModel(
                    () => App.GetNavigationService().NavigateTo("login"),
                    username
                );
    
                // Set DataContext
                this.DataContext = viewModel;
            };
        }
        
        protected override UserControl CreateSidebar()
        {
            return new AdminSidebarView();
        }
        
        protected override UserControl CreateContent()
        {
            return new AdminContentView();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}