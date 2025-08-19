using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
    public partial class EditEmployeeWindow : Window, INotifyPropertyChanged
    {
        private User _originalUser;
        
        // Properties with change notification
        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }
        
        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }
        
        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        
        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        
        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }
        
        private Department _selectedDepartment;
        public Department SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                {
                    UpdateAvailableRoles();
                }
            }
        }
        
        private string _selectedRole;
        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }
        
        private ObservableCollection<Department> _departments;
        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }
        
        private ObservableCollection<string> _availableRoles;
        public ObservableCollection<string> AvailableRoles
        {
            get => _availableRoles;
            set => SetProperty(ref _availableRoles, value);
        }
        
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
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
        
        // Constructor
        public EditEmployeeWindow(User userToEdit)
        {
            InitializeComponent();
            
            ErrorHandler.TrySafe("EditEmployeeWindow.Initialize", () => 
            {
                _originalUser = userToEdit;
                
                // Initialize collections
                Departments = new ObservableCollection<Department>(DepartmentService.GetAllDepartments());
                AvailableRoles = new ObservableCollection<string>();
                
                this.DataContext = this;
                
                // Populate form AFTER DataContext is set
                PopulateFormWithUserData(userToEdit);
                
                // Wire up buttons
                var cancelButton = this.FindControl<Button>("CancelButton");
                var updateButton = this.FindControl<Button>("UpdateButton");
                
                if (cancelButton != null)
                    cancelButton.Click += (s, e) => this.Close(null);
                    
                if (updateButton != null)
                    updateButton.Click += UpdateButton_Click;
                
                SetupEnhancedFocusManagement();
                
                Console.WriteLine($"✅ EditEmployeeWindow initialized for: {userToEdit.Username}");
                Console.WriteLine($"🏢 Selected Department: {SelectedDepartment?.Name}");
                Console.WriteLine($"👤 Selected Role: {SelectedRole}");
            });
        }
        
        private void PopulateFormWithUserData(User user)
        {
            Console.WriteLine($"🔄 Populating form for user: {user.Username}");
            Console.WriteLine($"🏢 User department: {user.Department}");
            Console.WriteLine($"👤 User role: {user.Role}");
            
            // Populate basic fields
            FirstName = user.Name ?? "";
            LastName = user.LastName ?? "";
            Username = user.Username ?? "";
            Password = user.Password ?? "";
            Email = user.Email ?? "";
            Phone = user.Phone ?? "";
            
            // Set department first, then role
            // Find the correct UI department for this user's role
            var matchingDepartment = Departments.FirstOrDefault(d => 
                d.AllowedRoles.Contains(user.Role));
            
            if (matchingDepartment != null)
            {
                Console.WriteLine($"✅ Found matching department: {matchingDepartment.Name} for role: {user.Role}");
                SelectedDepartment = matchingDepartment;
                
                // Set role after department is selected (this ensures AvailableRoles is populated)
                SelectedRole = user.Role;
                
                Console.WriteLine($"✅ Set SelectedRole to: {SelectedRole}");
                Console.WriteLine($"✅ Available roles count: {AvailableRoles?.Count ?? 0}");
            }
            else
            {
                Console.WriteLine($"⚠️ No matching department found for role: {user.Role}");
                // Fallback to first department
                if (Departments.Any())
                {
                    SelectedDepartment = Departments.First();
                    SelectedRole = AvailableRoles?.FirstOrDefault();
                }
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            await ErrorHandler.TrySafeAsync("EditEmployee.Update", async () =>
            {
                // Validation
                if (string.IsNullOrWhiteSpace(FirstName))
                {
                    ErrorMessage = "First name is required";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "Username is required";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Password is required";
                    return;
                }

                if (SelectedDepartment == null)
                {
                    ErrorMessage = "Department is required";
                    return;
                }

                if (string.IsNullOrWhiteSpace(SelectedRole))
                {
                    ErrorMessage = "Role is required";
                    return;
                }

                // Check connectivity
                await CheckConnectivityBeforeSave();

                // Get real department mapping
                int realDepartmentId = GetRealDepartmentId(SelectedDepartment.Name, SelectedRole);
                string realDepartmentName = GetRealDepartmentName(SelectedDepartment.Name, SelectedRole);

                // Create updated user
                var updatedUser = new User
                {
                    Id = _originalUser.Id,
                    Name = FirstName,
                    LastName = LastName,
                    Username = Username,
                    Password = Password,
                    Email = Email,
                    Phone = Phone,
                    Department = realDepartmentName,
                    DepartmentId = realDepartmentId,
                    Role = SelectedRole,
                    Status = _originalUser.Status
                };

                Console.WriteLine($"🎯 Updating user with DepartmentId: {updatedUser.DepartmentId} ({updatedUser.Department})");

                var success = await UserService.UpdateUserAsync(updatedUser);

                if (success)
                {
                    Console.WriteLine($"✅ User {updatedUser.Username} updated successfully");
                    this.Close(updatedUser);
                }
                else
                {
                    Console.WriteLine($"❌ Failed to update user {updatedUser.Username}");
                    ErrorMessage = "Could not update employee. Please check database connection and try again.";
                }
            });
        }

        private int GetRealDepartmentId(string uiDepartmentName, string role)
        {
            return uiDepartmentName switch
            {
                "Administration" => 1,
                "Human Resources" => 2,
                "Warehouse Management" => 3,
                "Warehouse Operations" => 3,
                "Logistics Management" => 4,
                "Logistics Operations" => 4,
                _ => 1
            };
        }

        private string GetRealDepartmentName(string uiDepartmentName, string role)
        {
            return uiDepartmentName switch
            {
                "Administration" => "Administration",
                "Human Resources" => "Human Resources",
                "Warehouse Management" => "Warehouse",
                "Warehouse Operations" => "Warehouse",
                "Logistics Management" => "Logistics",
                "Logistics Operations" => "Logistics",
                _ => "Administration"
            };
        }
        
        private void UpdateAvailableRoles()
        {
            ErrorHandler.TrySafe("UpdateAvailableRoles", () => 
            {
                if (SelectedDepartment != null)
                {
                    var currentRole = SelectedRole; // Store current selection
                    
                    AvailableRoles.Clear();
                    foreach (var role in SelectedDepartment.AllowedRoles)
                    {
                        AvailableRoles.Add(role);
                    }
                    
                    // Only change role if it's not compatible with new department
                    if (!string.IsNullOrEmpty(currentRole) && AvailableRoles.Contains(currentRole))
                    {
                        SelectedRole = currentRole; // Keep current role if valid
                    }
                    else
                    {
                        SelectedRole = AvailableRoles.Count > 0 ? AvailableRoles[0] : null;
                    }
                    
                    Console.WriteLine($"🔄 Updated roles for {SelectedDepartment.Name}: {AvailableRoles.Count} roles");
                    Console.WriteLine($"👤 Selected role: {SelectedRole}");
                }
            });
        }
        
        private async Task CheckConnectivityBeforeSave()
        {
            var isConnected = await ConnectivityService.CheckAndNotifyConnectionAsync(this);
            if (!isConnected)
            {
                Console.WriteLine("⚠️ Updating employee in offline mode");
            }
        }

        private void SetupEnhancedFocusManagement()
        {
            this.Opened += (sender, e) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var firstNameTextBox = this.FindControl<TextBox>("FirstNameTextBox");
                    firstNameTextBox?.Focus();
                }, Avalonia.Threading.DispatcherPriority.Loaded);
            };

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
                            var updateButton = this.FindControl<Button>("UpdateButton");
                            updateButton?.Command?.Execute(null);
                            e.Handled = true;
                        }
                        break;
                }
            };
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}