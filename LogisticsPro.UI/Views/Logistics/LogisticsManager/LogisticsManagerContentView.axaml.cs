using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Logistics.LogisticsManager
{
    public partial class LogisticsManagerContentView : UserControl
    {
        public LogisticsManagerContentView()
        {
            InitializeComponent();

            this.AttachedToVisualTree += (s, e) =>
            {
                Console.WriteLine("LogisticsManagerContentView attached to visual tree");

                var viewModel = this.DataContext as LogisticsManagerDashboardViewModel;
                if (viewModel != null)
                {
                    Console.WriteLine($"LogisticsManagerContentView has valid DataContext: {viewModel.GetType().Name}");
                }
                else
                {
                    Console.WriteLine("ERROR: LogisticsManagerContentView has no DataContext");

                    var parent = this.Parent;
                    while (parent != null)
                    {
                        var parentViewModel = parent.DataContext as LogisticsManagerDashboardViewModel;
                        if (parentViewModel != null)
                        {
                            Console.WriteLine($"Found parent with LogisticsManagerDashboardViewModel: {parent.GetType().Name}");
                            this.DataContext = parentViewModel;
                            Console.WriteLine("Set LogisticsManagerContentView DataContext from parent");
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