using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.Models;
using LogisticsPro.UI.Services;

namespace LogisticsPro.UI.Views.HR.Sections
{
    public partial class AddEmployeeWindow : Window, INotifyPropertyChanged
    {
        // Compact property declarations
        private string _firstName = "", _lastName = "", _username = "", _password = "", _email = "", _phone = "", _selectedRole = "", _errorMessage = "";
        private Department _selectedDepartment;
        private ObservableCollection<Department> _departments;
        private ObservableCollection<string> _availableRoles;
        
        private readonly Dictionary<string, List<string>> _departmentRoles = new()
        {
            { "Administration", new List<string> { "Administrator" } },
            { "Human Resources", new List<string> { "HR Manager" } },
            { "Warehouse", new List<string> { "Warehouse Manager", "Warehouse Employee" } },
            { "Logistics", new List<string> { "Logistics Manager", "Logistics Employee" } }
        };

        // Properties with compact setters
        public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }
        public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }
        public string Username { get => _username; set => SetProperty(ref _username, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
        public string SelectedRole { get => _selectedRole; set => SetProperty(ref _selectedRole, value); }
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
        
        public Department SelectedDepartment 
        { 
            get => _selectedDepartment; 
            set { if (SetProperty(ref _selectedDepartment, value)) UpdateAvailableRoles(); }
        }
        
        public ObservableCollection<Department> Departments 
        { 
            get => _departments; 
            set => SetProperty(ref _departments, value); 
        }
        
        public ObservableCollection<string> AvailableRoles 
        { 
            get => _availableRoles; 
            set => SetProperty(ref _availableRoles, value); 
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public AddEmployeeWindow()
        {
            InitializeComponent();
            ErrorHandler.TrySafe("AddEmployeeWindow.Initialize", () => 
            {
                Departments = new ObservableCollection<Department>(DepartmentService.GetAllDepartments());
                AvailableRoles = new ObservableCollection<string>();
                this.DataContext = this;

                // Wire up buttons with lambdas
                this.FindControl<Button>("CancelButton")!.Click += (s, e) => this.Close(null);
                this.FindControl<Button>("AddButton")!.Click += AddButton_Click;

                // Pre-select first department
                if (Departments.Count > 0) SelectedDepartment = Departments[0];
                
                // Add enhanced features
                SetupEnhancedFocusManagement();
            });
            ErrorMessage = null;
        }

        private void UpdateAvailableRoles()
        {
            if (SelectedDepartment == null) return;
            
            AvailableRoles.Clear();
            foreach (var role in SelectedDepartment.AllowedRoles) AvailableRoles.Add(role);
            
            if (!string.IsNullOrEmpty(SelectedRole) && !AvailableRoles.Contains(SelectedRole))
                SelectedRole = null;
            
            if (AvailableRoles.Count > 0 && string.IsNullOrEmpty(SelectedRole))
                SelectedRole = AvailableRoles[0];
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            await ErrorHandler.TrySafeAsync("AddEmployee.Submit", async () =>
            {
                // Compact validation
                if (string.IsNullOrWhiteSpace(FirstName)) { ErrorMessage = "First name is required"; return; }
                if (string.IsNullOrWhiteSpace(Username)) { ErrorMessage = "Username is required"; return; }
                if (string.IsNullOrWhiteSpace(Password)) { ErrorMessage = "Password is required"; return; }
                if (SelectedDepartment == null) { ErrorMessage = "Department is required"; return; }
                if (string.IsNullOrWhiteSpace(SelectedRole)) { ErrorMessage = "Role is required"; return; }
                if (UserService.GetUserByUsername(Username) != null) { ErrorMessage = "Username already exists"; return; }

                int realDepartmentId = GetRealDepartmentId(SelectedDepartment.Name, SelectedRole);
                string realDepartmentName = GetRealDepartmentName(SelectedDepartment.Name, SelectedRole);

                
                // Check connectivity before saving
                await CheckConnectivityBeforeSave();

                // Create and save user
                var user = new User
                {
                    Name = FirstName, LastName = LastName, Username = Username, Password = Password,
                    Email = Email, Phone = Phone, Department = realDepartmentName,
                    DepartmentId = realDepartmentId, Role = SelectedRole, Status = "Active"
                };

                Console.WriteLine($"🎯 Creating user with DepartmentId: {user.DepartmentId} ({user.Department})");

                var createdUser = await UserService.AddUserAsync(user);
                if (createdUser != null) 
                {
                    Console.WriteLine($"✅ SUCCESS: User {createdUser.Username} created with ID: {createdUser.Id}");
            
                    // Return the user with correct ID from database
                    this.Close(createdUser); 
                }
                else 
                {
                    Console.WriteLine($"❌ FAILED: UserService.AddUserAsync returned null");
                    ErrorMessage = "Could not save employee. Please check database connection and try again.";
                }
            });
        }
        
        private int GetRealDepartmentId(string uiDepartmentName, string role)
        {
            // Map UI departments back to real database departments
            return uiDepartmentName switch
            {
                "Administration" => 1,
                "Human Resources" => 2,
                "Warehouse Management" => 3,        // Maps to "Warehouse" (ID 3)
                "Warehouse Operations" => 3,        // Maps to "Warehouse" (ID 3)
                "Logistics Management" => 4,        // Maps to "Logistics" (ID 4)
                "Logistics Operations" => 4,        // Maps to "Logistics" (ID 4)
                _ => 1 // Default to Administration
            };
        }
        
        private string GetRealDepartmentName(string uiDepartmentName, string role)
        {
            return uiDepartmentName switch
            {
                "Administration" => "Administration",
                "Human Resources" => "Human Resources",
                "Warehouse Management" => "Warehouse",      // Real name in database
                "Warehouse Operations" => "Warehouse",      // Real name in database
                "Logistics Management" => "Logistics",      // Real name in database
                "Logistics Operations" => "Logistics",      // Real name in database
                _ => "Administration"
            };
        }
        
        
        // Added connectivity checking to the existing window
        private async Task CheckConnectivityBeforeSave()
        {
            var isConnected = await ConnectivityService.CheckAndNotifyConnectionAsync(this);
            if (!isConnected)
            {
                Console.WriteLine("⚠️ Adding employee in offline mode");
            }
            else
            {
                Console.WriteLine("✅ Connected - proceeding with database save");
            }
        }

        // Enhanced focus management and larger window
        private void SetupEnhancedFocusManagement()
        {
            // Set initial focus to first name field
            this.Opened += (sender, e) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var firstNameTextBox = this.FindControl<TextBox>("FirstNameTextBox");
                    firstNameTextBox?.Focus();
                }, Avalonia.Threading.DispatcherPriority.Loaded);
            };

            // Handle keyboard shortcuts
            this.KeyDown += (sender, e) =>
            {
                switch (e.Key)
                {
                    case Avalonia.Input.Key.Escape:
                        this.Close(null);
                        e.Handled = true;
                        break;
                    case Avalonia.Input.Key.Enter:
                        if (!(e.Source is TextBox))
                        {
                            var addButton = this.FindControl<Button>("AddButton");
                            addButton?.Command?.Execute(null);
                            e.Handled = true;
                        }
                        break;
                }
            };
        }
        
        
        
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
    
}