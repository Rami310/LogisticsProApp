using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using Avalonia;
using LogisticsPro.UI.ViewModels;
using LogisticsPro.UI.Views.HR.Sections;

namespace LogisticsPro.UI.Views.HR
{
    public partial class HRContentView : UserControl
    {
        public HRContentView()
        {
            InitializeComponent();

            this.AttachedToVisualTree += (s, e) =>
            {
                Console.WriteLine("HRContentView attached to visual tree");

                // Get the data context
                var viewModel = this.DataContext as HRDashboardViewModel;
                if (viewModel != null)
                {
                    Console.WriteLine($"HRContentView has valid DataContext: {viewModel.GetType().Name}");

                    var contentControl = this.FindControl<ContentControl>("ContentControl");
                    if (contentControl != null)
                    {
                        Console.WriteLine(
                            $"ContentControl found, CurrentSectionView is {(viewModel.CurrentSectionView != null ? "set" : "null")}");

                        // If CurrentSectionView is null, try to set it
                        if (viewModel.CurrentSectionView == null)
                        {
                            // Force navigation to employees section
                            viewModel.NavigateToSectionCommand.Execute("Employees");
                            Console.WriteLine("Manually executed NavigateToSection command");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR: ContentControl not found in HRContentView");
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: HRContentView has no DataContext");

                    // Try to get the DataContext from the parent
                    var parent = this.Parent;
                    while (parent != null)
                    {
                        var parentViewModel = parent.DataContext as HRDashboardViewModel;
                        if (parentViewModel != null)
                        {
                            Console.WriteLine($"Found parent with HRDashboardViewModel: {parent.GetType().Name}");
                            this.DataContext = parentViewModel;
                            Console.WriteLine("Set HRContentView DataContext from parent");
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