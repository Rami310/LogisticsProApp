using System;
using Avalonia.Controls;
using LogisticsPro.UI.Views.Shared;

namespace LogisticsPro.UI.Infrastructure
{
    /// <summary>
    /// Base class for all dashboard views to reduce code duplication and standardize the approach.
    /// </summary>
    public abstract class BaseDashboardView : UserControl
    {
        protected readonly string Title;
        
        /// <summary>
        /// Initializes a new instance of the BaseDashboardView class.
        /// </summary>
        /// <param name="title">The dashboard title to display</param>
        protected BaseDashboardView(string title)
        {
            Title = title;
            
            this.AttachedToVisualTree += (sender, e) => {
                SetupDashboard();
            };
        }
        
        /// <summary>
        /// Creates the sidebar specific to each dashboard implementation.
        /// </summary>
        /// <returns>The sidebar user control</returns>
        protected abstract UserControl CreateSidebar();
        
        /// <summary>
        /// Creates the main content specific to each dashboard implementation.
        /// </summary>
        /// <returns>The content user control</returns>
        protected abstract UserControl CreateContent();

        /// <summary>
        /// Sets up the dashboard with title, sidebar, and content components.
        /// </summary>
        private void SetupDashboard()
        {
            ErrorHandler.TrySafe($"{GetType().Name}.SetupDashboard", () =>
            {
                Console.WriteLine($"Setting up dashboard for {GetType().Name}");

                var layout = this.FindControl<DashboardLayout>("Layout");
                if (layout == null)
                {
                    ErrorHandler.LogError("SetupDashboard", new InvalidOperationException("Layout control not found"));
                    Console.WriteLine("ERROR: Layout control not found");
                    return;
                }

                var dashboardTitle = layout.FindControl<TextBlock>("DashboardTitle");
                if (dashboardTitle != null)
                {
                    dashboardTitle.Text = Title;
                    Console.WriteLine($"Set dashboard title to: {Title}");
                }

                var sidebar = CreateSidebar();
                var content = CreateContent();

                Console.WriteLine($"Created sidebar ({sidebar.GetType().Name}) and content ({content.GetType().Name})");

                var sidebarContent = layout.FindControl<ContentControl>("SidebarContent");
                var mainContent = layout.FindControl<ContentControl>("MainContent");

                Console.WriteLine(
                    $"Dashboard DataContext is: {(this.DataContext != null ? this.DataContext.GetType().Name : "NULL")}");

                if (this.DataContext != null)
                {
                    sidebar.DataContext = this.DataContext;
                    content.DataContext = this.DataContext;
                    Console.WriteLine("Explicitly set DataContext on sidebar and content views");
                }
                else
                {
                    Console.WriteLine("WARNING: Cannot set DataContext on child views - parent DataContext is null");
                }

                if (sidebarContent != null)
                {
                    sidebarContent.Content = sidebar;
                    Console.WriteLine("Set sidebar content");
                }
                else
                {
                    Console.WriteLine("ERROR: SidebarContent control not found");
                }

                if (mainContent != null)
                {
                    mainContent.Content = content;
                    Console.WriteLine("Set main content");
                }
                else
                {
                    Console.WriteLine("ERROR: MainContent control not found");
                }

                Console.WriteLine("Dashboard setup complete");
            });
        }
    }
}