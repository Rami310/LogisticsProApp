using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.Models;
using LogisticsPro.UI.Services;
using LogisticsPro.UI.ViewModels.Shared;
using LogisticsPro.UI.Views.Admin.Sections;

namespace LogisticsPro.UI.ViewModels
{
    public partial class AdminDashboardViewModel : BaseDashboardViewModel
    {
        // ========================================
        // REVENUE INTEGRATION - Same as other managers
        // ========================================
        [ObservableProperty] private BaseRevenueViewModel _revenueViewModel;

        // ========================================
        // WAREHOUSE STATISTICS (from Warehouse Manager)
        // ========================================
        [ObservableProperty] private int _totalStockQuantity;
        [ObservableProperty] private int _pendingRequests;
        [ObservableProperty] private int _lowStockItems;
        [ObservableProperty] private int _approvedRequests;
        [ObservableProperty] private string _availableBudgetDisplay = "$0";

        // ========================================
        // LOGISTICS STATISTICS (from Logistics Manager)
        // ========================================
        [ObservableProperty] private int _totalLogisticsRequests;
        [ObservableProperty] private int _inInventoryRequests;
        [ObservableProperty] private int _readyForShipmentRequests;

        // ========================================
        // EXISTING PROPERTIES 
        // ========================================
        [ObservableProperty] private ObservableCollection<RecentActivity> _recentActivities;
        [ObservableProperty] private string _currentSection = "Dashboard";
        [ObservableProperty] private Control _currentSectionView;
        [ObservableProperty] private ObservableCollection<string> _recentReports;
        [ObservableProperty] private bool _darkMode = false;
        [ObservableProperty] private bool _isLoading;
        
        public AdminDashboardViewModel(Action navigateToLogin, string username)
            : base(navigateToLogin, username, "Admin Dashboard")
        {
            // Initialize Revenue ViewModel (same pattern as other managers)
            RevenueViewModel = new BaseRevenueViewModel("Administrator");
            
            // Initialize mock data
            InitializeData();
            
            // Initialize reports data
            RecentReports = new ObservableCollection<string>
            {
                "Delivery Performance Report - March 2025",
                "Inventory Status Report - February 2025", 
                "Cost Analysis Report - Q1 2025",
                "Carrier Performance Report - January 2025"
            };
            
            // Load real data immediately (same as other managers)
            _ = Task.Run(async () => 
            {
                await Task.Delay(500);
                try 
                {
                    await LoadDashboardDataAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Admin dashboard loading failed: {ex.Message}");
                }
            });
        }

        // ========================================
        // REAL DATA LOADING (same pattern as other managers)
        // ========================================
        private async Task LoadDashboardDataAsync()
        {
            IsLoading = true;

            try
            {
                Console.WriteLine("Loading admin dashboard data...");

                await ErrorHandler.TrySafeAsync("LoadAdminDashboardData", async () =>
                {
                    // Load revenue data (same as other managers)
                    await RevenueViewModel.LoadRevenueDataAsync();

                    // Load warehouse statistics
                    await LoadWarehouseStatisticsAsync();

                    // Load logistics statistics  
                    await LoadLogisticsStatisticsAsync();

                    // Update order progress for admin overview
                    await UpdateAdminOrderProgressAsync();

                    Console.WriteLine("Admin dashboard data loaded successfully");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadAdminDashboardData error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadWarehouseStatisticsAsync()
        {
            try
            {
                // Get inventory data (same as warehouse manager)
                var inventory = await InventoryService.GetAllInventoryAsync();
                var totalStock = inventory.Sum(i => i.QuantityInStock);
                
                // Get product requests data
                var allRequests = await ProductRequestService.GetAllRequestsAsync();
                var pending = allRequests.Count(r => r.RequestStatus == "Pending");
                var approved = allRequests.Count(r => r.RequestStatus == "Approved");
                
                // Get low stock items (quantity < 10, same threshold as warehouse manager)
                var lowStock = inventory.Count(i => i.QuantityInStock < 10);

                // Update UI on main thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TotalStockQuantity = totalStock;
                    PendingRequests = pending;
                    LowStockItems = lowStock;
                    ApprovedRequests = approved;
                    
                    // Format budget display (same as warehouse manager)
                    var availableBudget = RevenueViewModel.AvailableBudget;
                    AvailableBudgetDisplay = availableBudget >= 1000000 
                        ? $"${availableBudget / 1000000:F0}M"
                        : availableBudget >= 1000
                        ? $"${availableBudget / 1000:F0}K" 
                        : $"${availableBudget:F0}";

                    Console.WriteLine($"Warehouse stats - Stock: {TotalStockQuantity}, Pending: {PendingRequests}, Low Stock: {LowStockItems}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadWarehouseStatistics error: {ex.Message}");
            }
        }

        private async Task LoadLogisticsStatisticsAsync()
        {
            try
            {
                // Get all requests for logistics overview (same as logistics manager)
                var allRequests = await ProductRequestService.GetAllRequestsAsync();
                var total = allRequests.Count;
                var inInventory = allRequests.Count(r => r.RequestStatus == "Approved");
                var readyForShipment = allRequests.Count(r => r.RequestStatus == "Ready for Shipment");

                // Update UI on main thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TotalLogisticsRequests = total;
                    InInventoryRequests = inInventory;
                    ReadyForShipmentRequests = readyForShipment;

                    Console.WriteLine($"Logistics stats - Total: {TotalLogisticsRequests}, Inventory: {InInventoryRequests}, Ready: {ReadyForShipmentRequests}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadLogisticsStatistics error: {ex.Message}");
            }
        }

        // ========================================
        // REFRESH DATA COMMAND (same as other managers)
        // ========================================
        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            await LoadDashboardDataAsync();
        }

        // ========================================
        // EXISTING METHODS (keep these unchanged)
        // ========================================
        private void InitializeData()
        {
            RecentActivities = new ObservableCollection<RecentActivity>
            {
                new RecentActivity
                {
                    ActivityType = ActivityType.Approved,
                    Description = "John approved a task",
                    Details = "Shipping confirmation for order #4587",
                    Timestamp = DateTime.Now.AddHours(-2)
                },
                new RecentActivity
                {
                    ActivityType = ActivityType.Login,
                    Description = "Jane logged in", 
                    Details = "User login from new device",
                    Timestamp = DateTime.Now.AddHours(-3)
                },
                new RecentActivity
                {
                    ActivityType = ActivityType.NewOrder,
                    Description = "New order #12345 received",
                    Details = "$3,456 - 23 items from Acme Corp",
                    Timestamp = DateTime.Now.AddDays(-1)
                }
            };
        }

        [RelayCommand]
        private void NavigateToSection(string section)
        {
            CurrentSection = section;
            
            switch (section)
            {
                case "Dashboard":
                    CurrentSectionView = null;
                    break;
                case "Reports":
                    CurrentSectionView = new ReportsSection();
                    break;
                case "Activity":
                    CurrentSectionView = new ActivitySection { DataContext = this };
                    break;
                default:
                    CurrentSectionView = null;
                    break;
            }
        }

        // Keep all other existing commands...
        [RelayCommand] private void ViewAllActivities() => Console.WriteLine("Navigate to detailed activities view");
        [RelayCommand] private void GenerateReport() => Console.WriteLine($"Generating report");
        [RelayCommand] private void ViewReport(string reportName) => Console.WriteLine($"Viewing report: {reportName}");
        [RelayCommand] private void ChangePassword() => Console.WriteLine("Change password dialog would open here");
        [RelayCommand] private void SaveAccountSettings() => Console.WriteLine("Account settings saved");
        [RelayCommand] private void SaveAppSettings() => Console.WriteLine("Application settings saved");
        [RelayCommand] private void CheckForUpdates() => Console.WriteLine("Checking for updates...");
        
        
        private async Task UpdateAdminOrderProgressAsync()
        {
            try
            {
                var allRequests = await ProductRequestService.GetAllRequestsAsync();
        
                var totalOrders = allRequests.Count;
                var failedOrders = allRequests.Count(r => r.RequestStatus == "Rejected" || r.RequestStatus == "Cancelled");
                var successfulOrders = totalOrders - failedOrders;
        
                // Check if RevenueViewModel exists before calling
                if (RevenueViewModel != null)
                {
                    RevenueViewModel.UpdateOrderProgress(totalOrders, successfulOrders, failedOrders);
                    Console.WriteLine($"Admin Success Rate: {successfulOrders}/{totalOrders} orders didn't fail ({(double)successfulOrders/totalOrders*100:F1}% success rate)");
                }
                else
                {
                    Console.WriteLine("RevenueViewModel is null in Admin dashboard");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateAdminOrderProgress error: {ex.Message}");
            }
        }
        
    }

    // Keep existing RecentActivity classes unchanged...
}

    // Keep existing RecentActivity classes...
    public enum ActivityType { Approved, Login, NewOrder, Error, Warning }

    public class RecentActivity
    {
        public ActivityType ActivityType { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }

        public string FormattedTimestamp
        {
            get
            {
                if (Timestamp.Date == DateTime.Today)
                    return $"Today, {Timestamp:h:mm tt}";
                else if (Timestamp.Date == DateTime.Today.AddDays(-1))
                    return $"Yesterday, {Timestamp:h:mm tt}";
                else
                    return Timestamp.ToString("MMM d, h:mm tt");
            }
        }

        public string IconText
        {
            get
            {
                return ActivityType switch
                {
                    ActivityType.Approved => "✓",
                    ActivityType.Login => "→",
                    ActivityType.NewOrder => "!",
                    ActivityType.Error => "X",
                    ActivityType.Warning => "!",
                    _ => "*"
                };
            }
        }

        public string IconColor
        {
            get
            {
                return ActivityType switch
                {
                    ActivityType.Approved => "#27AE60",
                    ActivityType.Login => "#3498DB",
                    ActivityType.NewOrder => "#F39C12",
                    ActivityType.Error => "#E74C3C",
                    ActivityType.Warning => "#F39C12",
                    _ => "#95A5A6"
                };
            }
            
        }
        
    }