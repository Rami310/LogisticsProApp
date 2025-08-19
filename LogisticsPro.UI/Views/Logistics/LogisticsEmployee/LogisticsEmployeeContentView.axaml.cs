using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Logistics.LogisticsEmployee
{
    public partial class LogisticsEmployeeContentView : UserControl
    {
        public LogisticsEmployeeContentView()
        {
            InitializeComponent();

            // Use Initialized instead of AttachedToVisualTree for better timing
            this.Initialized += (s, e) =>
            {
                Console.WriteLine("✅ LogisticsEmployeeContentView initialized");

                // Get the data context
                var viewModel = this.DataContext as LogisticsEmployeeDashboardViewModel;
                if (viewModel != null)
                {
                    Console.WriteLine($"✅ LogisticsEmployeeContentView has valid DataContext: {viewModel.GetType().Name}");
                }
                else
                {
                    Console.WriteLine("⚠️ LogisticsEmployeeContentView has no DataContext - will inherit from parent");

                    // Try to get the DataContext from the parent
                    var parent = this.Parent;
                    while (parent != null)
                    {
                        var parentViewModel = parent.DataContext as LogisticsEmployeeDashboardViewModel;
                        if (parentViewModel != null)
                        {
                            Console.WriteLine($"✅ Found parent with LogisticsEmployeeDashboardViewModel: {parent.GetType().Name}");
                            this.DataContext = parentViewModel;
                            Console.WriteLine("✅ Set LogisticsEmployeeContentView DataContext from parent");
                            break;
                        }

                        parent = parent is Visual visualParent ? visualParent.Parent : null;
                    }
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}