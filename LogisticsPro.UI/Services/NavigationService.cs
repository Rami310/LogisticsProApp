using System;
using System.Linq;
using Avalonia.Controls;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.ViewModels;
using LogisticsPro.UI.Views;
using LogisticsPro.UI.Views.Admin;
using LogisticsPro.UI.Views.HR;
using LogisticsPro.UI.Views.Logistics;
using LogisticsPro.UI.Views.Warehouse.WarehouseEmployee;
using LogisticsEmployeeDashboardView = LogisticsPro.UI.Views.Logistics.LogisticsEmployee.LogisticsEmployeeDashboardView;
using LogisticsPro.UI.Models;
using LogisticsPro.UI.Views.Logistics.LogisticsManager;
using LogisticsPro.UI.Views.Warehouse.WarehouseManager;

namespace LogisticsPro.UI.Services
{
    public class NavigationService
    {
        private readonly Window _mainWindow;
    
        public NavigationService(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void NavigateTo(string route, string parameter = "")
        {
            ErrorHandler.TrySafe($"NavigateTo_{route}", () =>
            {
                Console.WriteLine($"NavigateTo called with route: {route}, parameter: {parameter}");

                object content = null;

                switch (route.ToLower())
                {
                    case "welcome":
                        content = new WelcomeView();
                        content.GetType().GetProperty("DataContext")?.SetValue(
                            content, 
                            new MainWindowViewModel(() => NavigateTo("login"))
                        );
                        break;
                
                    case "login":
                        content = new LoginView();
                        content.GetType().GetProperty("DataContext")?.SetValue(
                            content, 
                            new LoginViewModel(
                                NavigateToDashboard,
                                () => NavigateTo("welcome")
                            )
                        );
                        break;

                    case "hr":
                        Console.WriteLine("Creating HR Dashboard View");
                        content = new HRDashboardView();
                        if (content is Control hrControl)
                        {
                            hrControl.Tag = parameter;
                            Console.WriteLine($"HR Dashboard Tag set to: {parameter}");
                        }
                        break;

                    case "admin":
                        Console.WriteLine("Creating Admin Dashboard View");
                        content = new AdminDashboardView();
                        if (content is Control adminControl)
                        {
                            adminControl.Tag = parameter;
                            Console.WriteLine($"Admin Dashboard Tag set to: {parameter}");
                        }
                        break;

                    case "warehouse":
                        Console.WriteLine("Creating Warehouse Dashboard View");
                        content = new WarehouseManagerDashboardView();
                        if (content is Control warehouseControl)
                        {
                            warehouseControl.Tag = parameter;
                            Console.WriteLine($"Warehouse Dashboard Tag set to: {parameter}");
                        }
                        break;
                    
                    case "logistics":
                        Console.WriteLine("Creating Logistics Manager Dashboard View");
                        content = new LogisticsManagerDashboardView();
                        if (content is Control logisticsControl)
                        {
                            logisticsControl.Tag = parameter;
                            Console.WriteLine($"Logistics Dashboard Tag set to: {parameter}");
                        }
                        break;

                    case "logistics_employee":
                        Console.WriteLine("Creating Logistics Employee Dashboard View");
                        content = new LogisticsEmployeeDashboardView();
                        if (content is Control logisticsEmpControl)
                        {
                            logisticsEmpControl.Tag = parameter;
                            Console.WriteLine($"Logistics Employee Dashboard Tag set to: {parameter}");
                        }
                        break;
                    
                    case "warehouse_employee":
                        Console.WriteLine("Creating Warehouse Employee Dashboard View");
                        content = new WarehouseEmployeeDashboardView();
                        if (content is Control warehouseEmpControl)
                        {
                             warehouseEmpControl.Tag = parameter;
                             Console.WriteLine($"Warehouse Employee Dashboard Tag set to: {parameter}");
                        }
                        break;
                    
                    case "employee":
                        Console.WriteLine("Creating Warehouse Employee Dashboard View");
                        content = new WarehouseEmployeeDashboardView();
                        if (content is Control employeeControl)
                        {
                            employeeControl.Tag = parameter;
                            Console.WriteLine($"Warehouse Employee Dashboard Tag set to: {parameter}");
                        }
                        break;

                    default:
                        Console.WriteLine($"Unknown route: {route}");
                        break;
                }

                // Set the content of the main window
                if (content != null)
                {
                    _mainWindow.Content = content;
                    Console.WriteLine($"Successfully navigated to {route}");
                }
                else
                {
                    Console.WriteLine($"Failed to create content for route: {route}");
                }
            });
        }

        public void NavigateToDashboard(User user)
        {
            ErrorHandler.TrySafe("NavigateToDashboard_User", () =>
            {
                if (user == null)
                {
                    Console.WriteLine("❌ User object is null. Redirecting to login.");
                    NavigateTo("login");
                    return;
                }

                Console.WriteLine($"Navigating Dashboard - Username: {user.Username}");
                Console.WriteLine($"User Role: {user.Role}");

                // SECURITY: Validate role before navigation
                var allowedRoles = new[]
                {
                    "Administrator",
                    "HR Manager",
                    "Warehouse Manager",
                    "Logistics Manager",
                    "Warehouse Employee",
                    "Logistics Employee"
                };

                if (string.IsNullOrWhiteSpace(user.Role) || !allowedRoles.Any(role => role == user.Role.Trim()))                {
                    Console.WriteLine($"🚨 SECURITY VIOLATION: Invalid role '{user.Role}' for user '{user.Username}'");
                    Console.WriteLine("🔒 Access denied - redirecting to login");
                    NavigateTo("login");
                    return;
                }

                // Navigate to appropriate dashboard based on role
                switch (user.Role?.Trim())
                {
                    case "Administrator":
                        Console.WriteLine("Attempting to navigate to Admin dashboard");
                        NavigateTo("admin", user.Username);
                        break;

                    case "HR Manager":
                        Console.WriteLine("Attempting to navigate to HR dashboard");
                        NavigateTo("hr", user.Username);
                        break;

                    case "Warehouse Manager":
                        Console.WriteLine("Attempting to navigate to Warehouse dashboard");
                        NavigateTo("warehouse", user.Username);
                        break;

                    case "Logistics Manager":
                        Console.WriteLine("Attempting to navigate to Logistics Manager dashboard");
                        NavigateTo("logistics", user.Username);
                        break;

                    case "Warehouse Employee":
                        Console.WriteLine("Attempting to navigate to Warehouse Employee dashboard");
                        NavigateTo("warehouse_employee", user.Username);
                        break;

                    case "Logistics Employee":
                        Console.WriteLine("Attempting to navigate to Logistics Employee dashboard");
                        NavigateTo("logistics_employee", user.Username);
                        break;

                    default:
                        Console.WriteLine($"🚨 SECURITY: Unknown role '{user.Role}' detected");
                        Console.WriteLine("🔒 Access denied for security - logging out user");

                        // Log security incident
                        Console.WriteLine(
                            $"🚨 SECURITY LOG: User '{user.Username}' with invalid role '{user.Role}' attempted access at {DateTime.Now}");

                        // Redirect to login instead of granting admin access
                        NavigateTo("login");
                        break;
                }
            });
        }

        //backward compatibility (calls UserService)
        public void NavigateToDashboard(string username)
        {
            ErrorHandler.TrySafe("NavigateToDashboard_String", () =>
            {
                // Get the user's role from UserService (mock data)
                var user = UserService.GetUserByUsername(username);
                if (user == null)
                {
                    Console.WriteLine("User not found. Redirecting to login.");
                    NavigateTo("login");
                    return;
                }

                // Call the new method with User object
                NavigateToDashboard(user);
            });
        }
    }
}