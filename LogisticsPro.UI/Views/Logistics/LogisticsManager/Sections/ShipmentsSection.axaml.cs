using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Logistics.LogisticsManager.Sections
{
    public partial class ShipmentsSection : UserControl
    {
        public ShipmentsSection()
        {
            InitializeComponent();
            
            this.AttachedToVisualTree += (s, e) =>
            {
                Console.WriteLine("ShipmentsSection attached to visual tree");

                // Get the data context
                var viewModel = this.DataContext as LogisticsManagerDashboardViewModel;
                if (viewModel != null)
                {
                    Console.WriteLine($"ShipmentsSection has valid DataContext: {viewModel.GetType().Name}");
                }
                else
                {
                    Console.WriteLine("ShipmentsSection DataContext is null or not LogisticsManagerDashboardViewModel");
                    
                    // Try to get the DataContext from the parent
                    var parent = this.Parent;
                    while (parent != null)
                    {
                        var parentViewModel = parent.DataContext as LogisticsManagerDashboardViewModel;
                        if (parentViewModel != null)
                        {
                            Console.WriteLine($"Found parent with LogisticsManagerDashboardViewModel: {parent.GetType().Name}");
                            this.DataContext = parentViewModel;
                            Console.WriteLine("Set ShipmentsSection DataContext from parent");
                            break;
                        }

                        parent = parent is Avalonia.Visual visualParent ? visualParent.Parent : null;
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