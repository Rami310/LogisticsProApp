using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.Models;
using LogisticsPro.UI.Services;
using LogisticsPro.UI.Views.HR;
using LogisticsPro.UI.Views.HR.Sections;
using EditEmployeeWindow = LogisticsPro.UI.Views.HR.Sections.EditEmployeeWindow;

namespace LogisticsPro.UI.ViewModels
{
    public partial class HRDashboardViewModel : BaseDashboardViewModel
    {
        // Properties - Combined related ones
        [ObservableProperty] private string _currentSection = "Employees";
        [ObservableProperty] private Control? _currentSectionView;
        [ObservableProperty] private ObservableCollection<User> _employees, _allEmployees, _filteredEmployees;
        [ObservableProperty] private ObservableCollection<Department> _departments;
        [ObservableProperty] private ObservableCollection<string> _availableRoles;
        [ObservableProperty] private Department _selectedDepartment;
        [ObservableProperty] private User _newEmployee = new User(), _selectedEmployee;
        [ObservableProperty] private string _errorMessage = "", _connectivityMessage = "";
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private int _itemsPerPage = 15;
        [ObservableProperty] private int _totalPages = 1;
        [ObservableProperty] private ObservableCollection<User> _paginatedEmployees = new();
        [ObservableProperty] private bool _isAddEmployeeDialogOpen,
            _isSelectAllChecked,
            _showBulkActions,
            _showConnectivityDialog,
            _isEditMode;

        [ObservableProperty] private User? _pendingUser;

        private string _searchText = "";

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterEmployees();
            }
        }


        public HRDashboardViewModel(Action navigateToLogin, string username) : base(navigateToLogin, username,
            "HR Dashboard")
        {
            InitializeCollections();
            _ = LoadDataAsync();
            NavigateToSection("Employees");
        }

        private void InitializeCollections()
        {
            Employees = new ObservableCollection<User>();
            AllEmployees = new ObservableCollection<User>();
            FilteredEmployees = new ObservableCollection<User>();
            Departments = new ObservableCollection<Department>();
            AvailableRoles = new ObservableCollection<string>();
        }

        // Data Loading
        private async Task LoadDataAsync()
        {
            try
            {
                await LoadEmployeesAsync();
                LoadDepartments();
                LoadRoles();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                ErrorHandler.LogError("LoadDataAsync", ex);
            }
        }

        public async Task LoadEmployeesAsync()
        {
            try
            {
                var employees = await UserService.GetAllEmployeesAsync();
                if (employees?.Any() == true)
                {
                    AllEmployees = new ObservableCollection<User>(employees);
                    FilteredEmployees = new ObservableCollection<User>(employees);
                    Employees.Clear();
                    foreach (var emp in employees) Employees.Add(emp);
                    if (!string.IsNullOrWhiteSpace(SearchText)) FilterEmployees();
                    else UpdatePagination(); // Update pagination even without search
                }
                else
                {
                    AllEmployees = new ObservableCollection<User>();
                    FilteredEmployees = new ObservableCollection<User>();
                    UpdatePagination();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employees: {ex.Message}");
                AllEmployees = new ObservableCollection<User>();
                FilteredEmployees = new ObservableCollection<User>();
                UpdatePagination();
            }
        }
        

        
        private void LoadDepartments()
        {
            Console.WriteLine("Loading departments with real-time employee data...");
            var departments = DepartmentService.GetAllDepartments();

            Console.WriteLine($"Found {departments?.Count ?? 0} departments from service");
            Console.WriteLine($"Current employee count in AllEmployees: {AllEmployees?.Count ?? 0}");

            if (departments?.Any() == true)
            {
                Departments.Clear();
                foreach (var dept in departments)
                {
                    // Use AllEmployees for most up-to-date data
                    dept.Employees = AllEmployees?.Where(emp => 
                        dept.AllowedRoles.Contains(emp.Role)).ToList() ?? new List<User>();
            
                    Console.WriteLine($"{dept.Name}: {dept.Employees.Count} employees");
                    foreach (var emp in dept.Employees)
                    {
                        Console.WriteLine($"   {emp.Name} ({emp.Role})");
                    }
    
                    Departments.Add(dept);
                }
        
                // Force UI notification that departments collection changed
                OnPropertyChanged(nameof(Departments));
        
                Console.WriteLine($"Departments collection updated with {Departments.Count} departments");
            }
        }

        
        // Auto-refresh data periodically
        private void StartAutoRefresh()
        {
            var timer = new System.Timers.Timer(30000); // 30 seconds
            timer.Elapsed += async (sender, e) =>
            {
                try
                {
                    await LoadEmployeesAsync();
                    LoadDepartments();
                    Console.WriteLine("Auto-refreshed employee and department data");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Auto-refresh error: {ex.Message}");
                }
            };
            timer.Start();
        }
        
        
        private void LoadRoles()
        {
            var roles = DepartmentService.SystemRoles;
            if (roles?.Any() == true)
            {
                AvailableRoles.Clear();
                foreach (var role in roles) AvailableRoles.Add(role);
            }
        }

        // Search
        private void FilterEmployees()
        {
            if (AllEmployees == null || FilteredEmployees == null) return;

            FilteredEmployees.Clear();
            var searchLower = SearchText?.ToLower();
            var filtered = string.IsNullOrWhiteSpace(searchLower)
                ? AllEmployees
                : AllEmployees.Where(e =>
                    e.Name?.ToLower().Contains(searchLower) == true ||
                    e.LastName?.ToLower().Contains(searchLower) == true ||
                    e.Username?.ToLower().Contains(searchLower) == true ||
                    e.Role?.ToLower().Contains(searchLower) == true ||
                    e.Department?.ToLower().Contains(searchLower) == true ||
                    e.Status?.ToLower().Contains(searchLower) == true);

            foreach (var emp in filtered) FilteredEmployees.Add(emp);
    
            // Call pagination after filtering
            UpdatePagination();
        }
        
        
        [RelayCommand]
        private void NextPage()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                UpdatePagination();
            }
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (CanGoPrevious)
            {
                CurrentPage--;
                UpdatePagination();
            }
        }
        
        // Commands
        [RelayCommand]
        private void Search() => FilterEmployees();

        [RelayCommand]
        private void ClearSearch() => SearchText = "";

        [RelayCommand]
        private async Task RefreshAsync() => await LoadEmployeesAsync();

        public async Task LoadEmployees() => await LoadEmployeesAsync();

        
        [RelayCommand]
        public async Task AddEmployeeAsync()
        {
            if (NewEmployee == null || string.IsNullOrWhiteSpace(NewEmployee.Username) ||
                string.IsNullOrWhiteSpace(NewEmployee.Name) || string.IsNullOrWhiteSpace(NewEmployee.Password))
            {
                ErrorMessage = "Please fill in all required fields";
                return;
            }

            try
            {
                await ConnectivityService.CheckAndNotifyConnectionAsync(GetMainWindow());

                var success = await UserService.AddUserAsync(NewEmployee);
                if (success != null)
                {
                    Console.WriteLine($"New employee added successfully: {success.Name} {success.LastName}");
            
                    ResetNewEmployee();
                    IsAddEmployeeDialogOpen = false;
                    ErrorMessage = "";
            
                    // Reload everything for real-time updates
                    await LoadEmployeesAsync();
            
                    // This was the key missing piece!
                    LoadDepartments(); // Refresh departments so new employee appears in department cards
            
                    Console.WriteLine($"Departments refreshed after adding employee: {success.Name}");
                }
                else
                {
                    PendingUser = NewEmployee;
                    ConnectivityMessage = "Connectivity issues - saved locally only";
                    ShowConnectivityDialog = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding employee: {ex.Message}";
            }
        }
        

        // Helper methods for real-time updates
        private async Task UpdateEmployeeInAllCollections(User updatedEmployee)
        {
            // Update in Employees collection
            var employeeIndex = Employees.ToList().FindIndex(e => e.Id == updatedEmployee.Id);
            if (employeeIndex >= 0)
            {
                Employees[employeeIndex] = updatedEmployee;
                Console.WriteLine($"Updated in Employees collection at index {employeeIndex}");
            }

            // Update in AllEmployees collection
            var allEmployeeIndex = AllEmployees.ToList().FindIndex(e => e.Id == updatedEmployee.Id);
            if (allEmployeeIndex >= 0)
            {
                AllEmployees[allEmployeeIndex] = updatedEmployee;
                Console.WriteLine($"Updated in AllEmployees collection at index {allEmployeeIndex}");
            }

            // Refresh filtered employees
            FilterEmployees();

            // Force property notifications
            OnPropertyChanged(nameof(Employees));
            OnPropertyChanged(nameof(AllEmployees));
            OnPropertyChanged(nameof(FilteredEmployees));
        }

        private async Task RemoveEmployeeFromAllCollections(User employee)
        {
            // Remove from all collections
            Employees.Remove(employee);
            AllEmployees.Remove(employee);
            FilteredEmployees.Remove(employee);

            // Force property notifications
            OnPropertyChanged(nameof(Employees));
            OnPropertyChanged(nameof(AllEmployees));
            OnPropertyChanged(nameof(FilteredEmployees));
        }

        
        [RelayCommand]
        public void ToggleAddEmployeeDialog() => IsAddEmployeeDialogOpen = !IsAddEmployeeDialogOpen;

        [RelayCommand]
        public void NavigateToSection(string section)
        {
            CurrentSection = section;
            CurrentSectionView = section switch
            {
                "Employees" => new EmployeesSection { DataContext = this },
                "Departments" => new DepartmentsSection { DataContext = this },
                _ => new EmployeesSection { DataContext = this }
            };
        }

        [RelayCommand]
        private void NavigateToEmployee(User employee)
        {
            if (employee == null) return;
    
            // Set the selected employee
            SelectedEmployee = employee;
    
            // Navigate to Employees section
            NavigateToSection("Employees");
    
            Console.WriteLine($"Navigating to employee: {employee.Name} {employee.LastName}");
        }

        // Department handling
        partial void OnSelectedDepartmentChanged(Department? value) => UpdateAvailableRoles();

        private void UpdateAvailableRoles()
        {
            if (SelectedDepartment == null) return;

            AvailableRoles.Clear();
            foreach (var role in SelectedDepartment.AllowedRoles) AvailableRoles.Add(role);

            if (NewEmployee != null)
            {
                NewEmployee.DepartmentId = SelectedDepartment.Id;
                NewEmployee.Department = SelectedDepartment.Name;
                if (!string.IsNullOrEmpty(NewEmployee.Role) && !AvailableRoles.Contains(NewEmployee.Role))
                    NewEmployee.Role = null;
            }
        }

        // Connectivity dialogs
        [RelayCommand]
        private void ConfirmLocalSave()
        {
            if (PendingUser == null) return;
    
            Console.WriteLine($"Confirming local save for: {PendingUser.Name}");
    
            ResetNewEmployee();
            IsAddEmployeeDialogOpen = false;
            ShowConnectivityDialog = false;
            PendingUser = null;
    
            // Refresh data after local save
            _ = LoadEmployeesAsync().ContinueWith(t => 
            {
                // After employees are loaded, refresh departments
                Avalonia.Threading.Dispatcher.UIThread.Invoke(() => 
                {
                    LoadDepartments();
                });
            });
        }

        [RelayCommand]
        private void CancelConnectivityDialog()
        {
            ShowConnectivityDialog = false;
            PendingUser = null;
        }

        // Bulk operations - Simplified
        [RelayCommand]
        private void ToggleSelectAll()
        {
            foreach (var emp in Employees) emp.IsSelected = IsSelectAllChecked;
            ShowBulkActions = Employees.Any(e => e.IsSelected);
        }

        [RelayCommand]
        private async Task BulkDeleteAsync()
        {
            var selected = Employees.Where(e => e.IsSelected).ToList();
            foreach (var emp in selected) Employees.Remove(emp);
            ShowBulkActions = false;
        }



        [RelayCommand]
        private async Task EditEmployeeAsync(User employee)
        {
            if (employee == null) return;

            try
            {
                Console.WriteLine($"Opening edit window for: {employee.Name} {employee.LastName}");

                // Check connectivity before opening edit window
                await ConnectivityService.CheckAndNotifyConnectionAsync(GetMainWindow());

                var editWindow = new EditEmployeeWindow(employee);
                var result = await editWindow.ShowDialog<User?>(GetMainWindow());

                if (result != null)
                {
                    Console.WriteLine($"Employee updated: {result.Name} {result.LastName}");

                    // Update all collections and refresh departments
                    await UpdateEmployeeInAllCollections(result);
                    
                    // Reload departments for real-time updates
                    LoadDepartments();

                    Console.WriteLine($"Real-time UI update completed for: {result.Name} {result.LastName}");
                }
                else
                {
                    Console.WriteLine("Edit cancelled");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Edit error: {ex.Message}");
                ErrorMessage = $"Edit error: {ex.Message}";
            }
        }


        [RelayCommand]
        private async Task DeleteEmployeeAsync(User employee)
        {
            if (employee == null) return;

            try
            {
                Console.WriteLine($"Delete requested: {employee.Name} {employee.LastName}");

                bool confirmed = await ShowImprovedDeleteConfirmation(employee);
                if (!confirmed)
                {
                    Console.WriteLine("Delete cancelled");
                    return;
                }

                Console.WriteLine("Attempting database deletion...");
                bool deletedFromDb = await UserService.DeleteUserAsync(employee.Id);
        
                if (deletedFromDb)
                {
                    // Remove from all collections and refresh departments
                    await RemoveEmployeeFromAllCollections(employee);
                    
                    // Reload departments for real-time updates
                    LoadDepartments();
                    
                    Console.WriteLine($"Deleted from database and UI: {employee.Name} {employee.LastName}");
                }
                else
                {
                    await ShowConnectionError(employee);
                    Console.WriteLine($"Database deletion failed for: {employee.Name} {employee.LastName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete error: {ex.Message}");
                ErrorMessage = $"Delete error: {ex.Message}";
            }
        }
        
    // Helper methods - Ultra simplified
        private Window? GetMainWindow() => 
            Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;

        private async Task<bool> CheckApiConnection()
        {
            try { var employees = await UserService.GetAllEmployeesAsync(); return employees != null; }
            catch { return false; }
        }

        private async Task<bool> ShowDeleteConfirmation(User employee)
        {
            var dialog = CreateDialog("Confirm Delete", 400, 200);
            var panel = new StackPanel { Margin = new Thickness(20), Spacing = 15 };
            
            panel.Children.Add(new TextBlock { Text = "Delete Employee", FontSize = 18, FontWeight = FontWeight.Bold, HorizontalAlignment = HorizontalAlignment.Center });
            panel.Children.Add(new TextBlock { Text = $"Delete {employee.Name} {employee.LastName}?", HorizontalAlignment = HorizontalAlignment.Center });
            panel.Children.Add(new TextBlock { Text = "This cannot be undone.", FontStyle = FontStyle.Italic, Foreground = Brushes.Red, HorizontalAlignment = HorizontalAlignment.Center });
            
            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Spacing = 10 };
            var cancel = CreateButton("Cancel", Brushes.Gray); cancel.Click += (s, e) => dialog.Close(false);
            var delete = CreateButton("Delete", Brushes.Red); delete.Click += (s, e) => dialog.Close(true);
            buttons.Children.Add(cancel); buttons.Children.Add(delete);
            panel.Children.Add(buttons);

            dialog.Content = panel;
            var result = await dialog.ShowDialog<bool?>(GetMainWindow());
            return result == true;
        }

        private async Task ShowConnectionError(User employee)
        {
            var dialog = CreateDialog("Connection Error", 400, 200);
            var panel = new StackPanel { Margin = new Thickness(20), Spacing = 15 };
            
            panel.Children.Add(new TextBlock { Text = "Connection Error", FontSize = 18, FontWeight = FontWeight.Bold, HorizontalAlignment = HorizontalAlignment.Center, Foreground = Brushes.Orange });
            panel.Children.Add(new TextBlock { Text = $"Cannot delete {employee.Name} {employee.LastName} - connection failed.", HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap });
            
            var ok = CreateButton("OK", Brushes.Blue); ok.Click += (s, e) => dialog.Close();
            panel.Children.Add(ok);

            dialog.Content = panel;
            await dialog.ShowDialog(GetMainWindow());
        }

        private Window CreateDialog(string title, double width, double height) => new()
        {
            Title = title, Width = width, Height = height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false, Background = Brushes.White
        };

        private Button CreateButton(string content, IBrush color) => new()
        {
            Content = content, Width = 80, Height = 30, Background = color,
            Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center
        };

        private void ResetNewEmployee()
        {
            NewEmployee = new User 
            { 
                Status = "Active", 
                DepartmentId = SelectedDepartment?.Id ?? 1, 
                Department = SelectedDepartment?.Name ?? "Administration" 
            };
            SelectedDepartment = Departments.FirstOrDefault();
            AvailableRoles.Clear();
            UpdateAvailableRoles();
            IsEditMode = false; // This ensures dialog shows "Add" mode next time
        }

        private async Task<bool> ShowImprovedDeleteConfirmation(User employee)
        {
            try
            {
                var dialog = new Window
                {
                    Title = "Confirm Delete",
                    Width = 450,
                    Height = 220,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = false,
                    Background = Brushes.White
                };

                var panel = new StackPanel
                {
                    Margin = new Thickness(25),
                    Spacing = 20
                };

                // Title
                panel.Children.Add(new TextBlock
                {
                    Text = "Delete Employee",
                    FontSize = 20,
                    FontWeight = FontWeight.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80))
                });

                // Main question
                panel.Children.Add(new TextBlock
                {
                    Text = $"Are you sure you want to delete {employee.Name} {employee.LastName}?",
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
                });

                // Warning
                panel.Children.Add(new TextBlock
                {
                    Text = "This action cannot be undone.",
                    FontSize = 12,
                    FontStyle = FontStyle.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(192, 57, 43)) // Matte red
                });

                // Buttons
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Spacing = 15
                };

                // Cancel button - Matte gray
                var cancelBtn = new Button
                {
                    Content = "Cancel",
                    Width = 100,
                    Height = 35,
                    Background = new SolidColorBrush(Color.FromRgb(127, 140, 141)), // Matte gray
                    Foreground = Brushes.White,
                    FontWeight = FontWeight.SemiBold,
                    CornerRadius = new CornerRadius(4),
                    BorderThickness = new Thickness(0)
                };
                cancelBtn.Click += (s, e) => dialog.Close(false);

                // Delete button - Matte red (less glossy)
                var deleteBtn = new Button
                {
                    Content = "Delete",
                    Width = 100,
                    Height = 35,
                    Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)), // Matte red
                    Foreground = Brushes.White,
                    FontWeight = FontWeight.SemiBold,
                    CornerRadius = new CornerRadius(4),
                    BorderThickness = new Thickness(0)
                };
                deleteBtn.Click += (s, e) => dialog.Close(true);

                buttonPanel.Children.Add(cancelBtn);
                buttonPanel.Children.Add(deleteBtn);
                panel.Children.Add(buttonPanel);

                dialog.Content = panel;
                var result = await dialog.ShowDialog<bool?>(GetMainWindow());
                return result == true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing delete confirmation: {ex.Message}");
                return false;
            }
        }
        
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        private void UpdatePagination()
        {
            if (FilteredEmployees == null || FilteredEmployees.Count == 0)
            {
                TotalPages = 1;
                CurrentPage = 1;
                PaginatedEmployees.Clear();
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));
                return;
            }

            // Calculate total pages
            TotalPages = (int)Math.Ceiling((double)FilteredEmployees.Count / ItemsPerPage);
    
            // Ensure current page is valid
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;

            // Calculate items for current page
            var startIndex = (CurrentPage - 1) * ItemsPerPage;
            var itemsToTake = Math.Min(ItemsPerPage, FilteredEmployees.Count - startIndex);
    
            // Update display IDs and create paginated list
            PaginatedEmployees.Clear();
            var pageItems = FilteredEmployees.Skip(startIndex).Take(itemsToTake).ToList();
    
            for (int i = 0; i < pageItems.Count; i++)
            {
                var employee = pageItems[i];
                // Use display order instead of database ID
                employee.DisplayId = startIndex + i + 1;
                PaginatedEmployees.Add(employee);
            }

            // Notify pagination button states
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
        }
        
    }
}