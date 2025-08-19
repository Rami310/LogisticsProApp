using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LogisticsPro.UI.ViewModels;
using LogisticsPro.UI.Views.Shared;

namespace LogisticsPro.UI.Views.Shared
{
    public partial class DashboardLayout : UserControl
    {
        public DashboardLayout()
        {
            InitializeComponent();

            // Start date/time update timer
            var timer = new System.Timers.Timer(30000); // Update every 30 seconds
            timer.Elapsed += (s, e) => 
            {
                Dispatcher.UIThread.InvokeAsync(() => {
                    UpdateDateTime();
                });
            };
            timer.Start();

            // Initial date/time update
            UpdateDateTime();
            
            // Subscribe to DataContext changes for sidebar state
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, EventArgs e)
        {
            if (DataContext is BaseDashboardViewModel viewModel)
            {
                // Subscribe to sidebar state changes
                viewModel.PropertyChanged += OnViewModelPropertyChanged;
                
                // Set initial state
                UpdateSidebarState(viewModel.IsSidebarExpanded);
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BaseDashboardViewModel.IsSidebarExpanded) && 
                sender is BaseDashboardViewModel viewModel)
            {
                UpdateSidebarState(viewModel.IsSidebarExpanded);
            }
        }

        private void UpdateSidebarState(bool isExpanded)
        {
            var sidebarContent = this.FindControl<ContentControl>("SidebarContent");
            if (sidebarContent?.Content is BaseSidebarView sidebar)
            {
                sidebar.SetCollapsedState(isExpanded);
            }
        }

        private void UpdateDateTime()
        {
            var currentDate = this.FindControl<TextBlock>("CurrentDate");
            var currentTime = this.FindControl<TextBlock>("CurrentTime");
            
            if (currentDate != null)
                currentDate.Text = DateTime.Now.ToString("dddd, MMMM d, yyyy");
            
            if (currentTime != null)
                currentTime.Text = DateTime.Now.ToString("h:mm tt");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}