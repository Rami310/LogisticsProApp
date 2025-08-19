using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.Models;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Warehouse.WarehouseManager.Sections
{
    public partial class ProductRequestsSection : UserControl
    {
        public ProductRequestsSection()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}