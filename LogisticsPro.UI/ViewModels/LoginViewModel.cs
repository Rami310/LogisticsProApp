using System;
using System.IO;
using Avalonia.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.Services;
using LogisticsPro.UI.Models;

namespace LogisticsPro.UI.ViewModels
{
    public partial class LoginViewModel : BaseValidatableViewModel
    {
        private readonly Action<User> _navigateToDashboardBasedOnRole;
        private readonly Action _navigateToWelcome;
        [ObservableProperty] private bool _isOfflineMode;
        [ObservableProperty] private string _offlineModeMessage = "";

        [ObservableProperty] private string _username = "";
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private bool _rememberMe = false;

        // UI text properties
        [ObservableProperty] private string _loginTitle = "Login";
        [ObservableProperty] private string _loginButtonText = "Login";
        [ObservableProperty] private string _backToWelcomeText = "Back to Welcome";
        [ObservableProperty] private string _rememberMeText = "Remember me";

        public LoginViewModel(Action<User> navigateToDashboardBasedOnRole, Action navigateToWelcome)
        {
            _navigateToDashboardBasedOnRole = navigateToDashboardBasedOnRole;
            _navigateToWelcome = navigateToWelcome;
            LoadSavedCredentials();
            
            // Test API connectivity (enhanced debugging)
            _ = Task.Run(async () =>
            {
                Console.WriteLine("Testing API connectivity...");
                Console.WriteLine($"API URL: https://localhost:7001");
        
                await IntegrationTestHelper.TestSpecificEndpointAsync("/health");
        
                var isConnected = await IntegrationTestHelper.QuickConnectivityTestAsync();
        
                if (isConnected)
                {
                    Console.WriteLine("SUCCESS: Ready for database authentication!");
                }
                else
                {
                    Console.WriteLine("Using mock data fallback");
                }
            });
        }

        // Default constructor for design time
        public LoginViewModel() : this(_ => { }, () => { }) { }

        [RelayCommand]
private async Task LoginAsync()
{
    // Only clear errors once, properly
    ClearAllErrors();

    // Validate username and password
    if (!ValidateRequired(Username, nameof(Username), "Username") ||
        !ValidateRequired(Password, nameof(Password), "Password"))
        return;
    
    // Check connectivity DURING login (safe on UI thread)
    _ = Task.Run(async () =>
    {
        await ConnectivityService.CheckAndNotifyConnectionAsync();
    });

    await ErrorHandler.TrySafeAsync("UserLogin", async () =>
    {
        IsLoading = true;
        LoginButtonText = "Logging in...";

        Console.WriteLine($"Login Attempt - Username: {Username}");

        try
        {
            // STEP 1: Try API authentication first
            var user = await UserService.ValidateUserAsync(Username, Password);

            if (user != null)
            {
                Console.WriteLine($"API Login successful! Role: {user.Role}");

                // Save credentials if remember me is checked
                if (RememberMe) SaveCredentials();

                _navigateToDashboardBasedOnRole(user);
                return;
            }

            Console.WriteLine("API authentication failed, trying mock login...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API login error: {ex.Message}");
            Console.WriteLine("Falling back to mock authentication...");
        }

        // STEP 2: Fallback to mock authentication
        var mockUser = UserService.ValidateUserMock(Username, Password);

        if (mockUser != null)
        {
            Console.WriteLine($"Mock Login successful! Role: {mockUser.Role}");
            Console.WriteLine("Note: Using mock data - no database connection");

            // Save credentials if remember me is checked
            if (RememberMe) SaveCredentials();

            _navigateToDashboardBasedOnRole(mockUser);
        }
        else
        {
            Console.WriteLine("Login failed: Invalid credentials");
    
            // Force immediate UI thread update with delay
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                // Clear any existing errors first
                ClearAllErrors();
        
                // Small delay to ensure UI state is clean
                await Task.Delay(50);
        
                // Add the error
                AddError(nameof(Username), "Invalid username or password. Please try again.");
        
                // Force property change notifications
                OnPropertyChanged(nameof(ErrorMessageText));
                OnPropertyChanged(nameof(HasError));
        
                Console.WriteLine($"UI Thread - HasError: {HasError}, ErrorMessage: '{ErrorMessageText}'");
            });

            // Show available test accounts
            Console.WriteLine("Available test accounts:");
            Console.WriteLine("   • admin / 1234 (Administrator)");
            Console.WriteLine("   • hrmanager / hr123 (HR Manager)");
            Console.WriteLine("   • warehouse_mgr / wh123 (Warehouse Manager)");
            Console.WriteLine("   • logistics_mgr / log123 (Logistics Manager)");
            Console.WriteLine("   • wh_emp1 / emp123 (Warehouse Employee)");
            Console.WriteLine("   • log_emp1 / emp123 (Logistics Employee)");
        }
    });

    // Reset loading state
    IsLoading = false;
    LoginButtonText = "Login";
}

        // Working Remember Me - Save Credentials
        private void SaveCredentials()
        {
            ErrorHandler.TrySafe("SaveCredentials", () => 
            {
                Console.WriteLine($"Saving credentials for user: {Username}");
                
                var appDataPath = GetAppDataPath();
                var credentialsFile = Path.Combine(appDataPath, "credentials.txt");
                
                // Simple encryption (base64) - good enough for demo
                var credentials = $"{Username}|{Password}";
                var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credentials));
                
                File.WriteAllText(credentialsFile, encoded);
                Console.WriteLine($"Credentials saved to: {credentialsFile}");
            });
        }

        // Working Remember Me - Load Saved Credentials  
        private void LoadSavedCredentials()
        {
            ErrorHandler.TrySafe("LoadCredentials", () => 
            {
                Console.WriteLine("Checking for saved credentials...");
                
                var appDataPath = GetAppDataPath();
                var credentialsFile = Path.Combine(appDataPath, "credentials.txt");
                
                if (File.Exists(credentialsFile))
                {
                    var encoded = File.ReadAllText(credentialsFile);
                    var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                    var parts = decoded.Split('|');
                    
                    if (parts.Length == 2)
                    {
                        Username = parts[0];
                        Password = parts[1];
                        RememberMe = true;
                        Console.WriteLine($"Loaded saved credentials for: {Username}");
                        
                        // Auto-login if credentials were found
                        _ = Task.Run(async () => 
                        {
                            await Task.Delay(500); // Small delay to let UI settle
                            await LoginAsync();
                        });
                    }
                }
                else
                {
                    Console.WriteLine("No saved credentials found");
                }
            });
        }

        // Helper method for cross-platform app data path
        private string GetAppDataPath()
        {
            var appName = "LogisticsPro";
            string appDataPath;
            
            if (OperatingSystem.IsWindows())
            {
                appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
            }
            else if (OperatingSystem.IsMacOS())
            {
                appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
            }
            else // Linux and others
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                appDataPath = Path.Combine(home, $".{appName.ToLower()}");
            }
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(appDataPath);
            return appDataPath;
        }

        // Clear saved credentials method
        [RelayCommand]
        private void ClearSavedCredentials()
        {
            ErrorHandler.TrySafe("ClearCredentials", () =>
            {
                var appDataPath = GetAppDataPath();
                var credentialsFile = Path.Combine(appDataPath, "credentials.txt");
                
                if (File.Exists(credentialsFile))
                {
                    File.Delete(credentialsFile);
                    Console.WriteLine("Saved credentials cleared");
                }
                
                RememberMe = false;
                Username = "";
                Password = "";
                Console.WriteLine("Login form cleared");
            });
        }
        
        [RelayCommand]
        private void NavigateToWelcome() => _navigateToWelcome?.Invoke();
        
        // Clear fields and errors
        [RelayCommand]
        private void ClearForm()
        {
            Username = "";
            Password = "";
            ClearAllErrors();
        }

        [RelayCommand]
        private void FillTestCredentials(string role)
        {
            switch (role.ToLower())
            {
                case "admin":
                    Username = "admin";
                    Password = "1234";
                    break;
                case "hr":
                    Username = "hrmanager";
                    Password = "hr123";
                    break;
                case "warehouse_manager":
                    Username = "warehouse_mgr";
                    Password = "wh123";
                    break;
                case "logistics_manager":
                    Username = "logistics_mgr";
                    Password = "log123";
                    break;
                case "warehouse_employee":
                    Username = "wh_emp1";
                    Password = "emp123";
                    break;
                case "logistics_employee":
                    Username = "log_emp1";
                    Password = "emp123";
                    break;
            }
            
            Console.WriteLine($"Test credentials filled for {role}");
        }

        // Quick API test command (for debugging)
        [RelayCommand]
        private async Task TestApiConnectionAsync()
        {
            await ErrorHandler.TrySafeAsync("TestApiConnection", async () =>
            {
                Console.WriteLine("Testing API connection...");
                
                var isConnected = await ApiConfiguration.IsApiAvailableAsync();
                
                if (isConnected)
                {
                    ErrorMessageText = "";
                    Console.WriteLine("API connection successful!");
                }
                else
                {
                    ErrorMessageText = "Unable to connect to API. Check if it's running at https://localhost:7001";
                    Console.WriteLine("API connection failed");
                }
            });
        }
    }
}