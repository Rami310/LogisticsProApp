using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Admin
{
    public partial class AdminContentView : UserControl
    {
        public AdminContentView()
        {
            InitializeComponent();

            this.AttachedToVisualTree += (s, e) =>
            {
                Console.WriteLine("AdminContentView attached to visual tree");

                var viewModel = this.DataContext as AdminDashboardViewModel;
                if (viewModel != null)
                {
                    Console.WriteLine($"AdminContentView has valid DataContext: {viewModel.GetType().Name}");
                }
                else
                {
                    Console.WriteLine("AdminContentView DataContext is null or not AdminDashboardViewModel");
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}