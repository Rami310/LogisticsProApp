using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.Models;
using LogisticsPro.UI.ViewModels;
using LogisticsPro.UI.Views.HR;
using System;
using Avalonia.Threading;

namespace LogisticsPro.UI.Views.HR.Sections
{
    public partial class EmployeesSection : UserControl
    {
        private HRDashboardViewModel _viewModel;
        private DataGrid _employeeDataGrid;
        
        public EmployeesSection()
        {
            InitializeComponent();
            
            this.DataContextChanged += EmployeesSection_DataContextChanged;
            this.AttachedToVisualTree += EmployeesSection_AttachedToVisualTree;
            
            // Find and wire up the Add Employee button
            var addButton = this.FindControl<Button>("AddEmployeeButton");
            if (addButton != null)
            {
                addButton.Click += AddEmployeeButton_Click;
                Console.WriteLine("Add Employee button event wired up successfully");
            }
            
            // Get reference to the DataGrid
            _employeeDataGrid = this.FindControl<DataGrid>("EmployeeDataGrid");
            if (_employeeDataGrid != null)
            {
                Console.WriteLine("Employee DataGrid found");
            }
            else
            {
                Console.WriteLine("ERROR: Employee DataGrid not found");
            }
        }
        
        private async void EmployeesSection_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            Console.WriteLine("EmployeesSection attached to visual tree");
            
            if (_viewModel != null)
            {
                await _viewModel.LoadEmployees();
            }
        }
        
        private async void EmployeesSection_DataContextChanged(object sender, EventArgs e)
        {
            _viewModel = DataContext as HRDashboardViewModel;
            Console.WriteLine($"EmployeesSection DataContext changed: {_viewModel != null}");
            
            if (_viewModel != null)
            {
                await _viewModel.LoadEmployees();
            }
        }

        private async void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
{
    Console.WriteLine("Add Employee button clicked!");
    
    try
    {
        var dialog = new AddEmployeeWindow();
        
        if (this.VisualRoot is Window parentWindow)
        {
            Console.WriteLine("🔄 About to show dialog...");
            var result = await dialog.ShowDialog<User?>(parentWindow);
            
            Console.WriteLine($"🔍 Dialog returned: {result != null}");
            if (result != null)
            {
                Console.WriteLine($"✅ Employee added: {result.Name} {result.LastName}");
                
                if (_viewModel != null)
                {
                    Console.WriteLine("📝 Adding to collections...");
                    
                    // Add to all collections
                    _viewModel.AllEmployees.Add(result);
                    _viewModel.Employees.Add(result);
                    
                    // Only add to filtered if it matches current search
                    if (string.IsNullOrWhiteSpace(_viewModel.SearchText) || 
                        result.Name?.ToLower().Contains(_viewModel.SearchText.ToLower()) == true ||
                        result.LastName?.ToLower().Contains(_viewModel.SearchText.ToLower()) == true ||
                        result.Username?.ToLower().Contains(_viewModel.SearchText.ToLower()) == true)
                    {
                        _viewModel.FilteredEmployees.Add(result);
                        Console.WriteLine("✅ Added to filtered employees");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Not added to filtered (doesn't match search)");
                    }
                    
                    Console.WriteLine($"📊 Collections now: All={_viewModel.AllEmployees.Count}, Filtered={_viewModel.FilteredEmployees.Count}");
                }
            }
            else
            {
                Console.WriteLine("❌ Dialog returned null - user was not created or dialog was cancelled");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}