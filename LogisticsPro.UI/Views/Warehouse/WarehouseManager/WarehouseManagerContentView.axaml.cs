using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Warehouse.WarehouseManager
{
    public partial class WarehouseManagerContentView : UserControl
    {
        public WarehouseManagerContentView()
        {
            InitializeComponent();

            this.AttachedToVisualTree += (s, e) =>
            {
                Console.WriteLine("WarehouseManagerContentView attached to visual tree");

                // Get the data context
                var viewModel = this.DataContext as WarehouseManagerDashboardViewModel;
                if (viewModel != null)
                {
                    Console.WriteLine($"WarehouseManagerContentView has valid DataContext: {viewModel.GetType().Name}");

                    var contentControl = this.FindControl<ContentControl>("ContentControl");
                    if (contentControl != null)
                    {
                        Console.WriteLine($"ContentControl found, CurrentSectionView is {(viewModel.CurrentSectionView != null ? "NOT NULL" : "NULL")}");
                    }
                    else
                    {
                        Console.WriteLine("ContentControl not found in WarehouseManagerContentView");
                    }
                }
                else
                {
                    Console.WriteLine("WarehouseManagerContentView DataContext is null or not HRDashboardViewModel");
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}