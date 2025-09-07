using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.Models;
using LogisticsPro.UI.Models.Revenue;
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
        
        
        private readonly IChartService _chartService;
        [ObservableProperty] private ISeries[] _profitChartSeries;
        [ObservableProperty] private Axis[] _profitChartXAxes;
        [ObservableProperty] private Axis[] _profitChartYAxes;
        
        // Income chart properties (for logistics perspective)
        [ObservableProperty] private ISeries[] _incomeChartSeries;
        [ObservableProperty] private Axis[] _incomeChartXAxes;
        [ObservableProperty] private Axis[] _incomeChartYAxes;

        // Spending chart properties (for warehouse perspective) 
        [ObservableProperty] private ISeries[] _spendingChartSeries;
        [ObservableProperty] private Axis[] _spendingChartXAxes;
        [ObservableProperty] private Axis[] _spendingChartYAxes;
        
        public AdminDashboardViewModel(Action navigateToLogin, string username)
            : base(navigateToLogin, username, "Admin Dashboard")
        {
            // Initialize chart service
            _chartService = ServiceLocator.Get<IChartService>();
            
            // Initialize Revenue ViewModel (same pattern as other managers)
            RevenueViewModel = new BaseRevenueViewModel("Administrator");
            
            // Initialize chart properties
            InitializeChartProperties();
            
            // Initialize mock data
            InitializeData();
            
            // Initialize localized texts
            UpdateLocalizedTexts();

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
        
        private async Task LoadAllThreeChartsAsync()
{
    Console.WriteLine("Loading all three charts for admin dashboard...");
    
    try
    {
        // Get transactions once and use for all three charts
        var response = await ApiConfiguration.HttpClient.GetAsync("Revenue/transactions");
        
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var transactions = JsonSerializer.Deserialize<List<RevenueTransactionDto>>(responseJson, ApiConfiguration.JsonOptions);
            
            if (transactions != null)
            {
                // Create all three charts using chart service
                var (spendingSeries, spendingXAxes, spendingYAxes) = _chartService.CreateSpendingChartFromTransactions(transactions);
                var (incomeSeries, incomeXAxes, incomeYAxes) = await _chartService.PrepareMonthlyProfitChartAsync();
                var (cleanProfitSeries, cleanProfitXAxes, cleanProfitYAxes) = _chartService.CreateCleanProfitChart(transactions);
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Update spending chart (red - money out)
                    SpendingChartSeries = spendingSeries;
                    SpendingChartXAxes = spendingXAxes;
                    SpendingChartYAxes = spendingYAxes;
                    
                    // Update income chart (green - money in)
                    IncomeChartSeries = incomeSeries;
                    IncomeChartXAxes = incomeXAxes;
                    IncomeChartYAxes = incomeYAxes;
                    
                    // Update clean profit chart (blue - net result)
                    ProfitChartSeries = cleanProfitSeries;
                    ProfitChartXAxes = cleanProfitXAxes;
                    ProfitChartYAxes = cleanProfitYAxes;
                    
                    Console.WriteLine("All three admin charts updated");
                });
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error loading admin charts: {ex.Message}");
    }
}

        
        private async Task PrepareMonthlyProfitChartAsync()
        {
            Console.WriteLine("Preparing chart using service...");

            try
            {
                // Use the service to prepare the chart for admin role
                var (series, xAxes, yAxes) = await _chartService.PrepareMonthlyProfitChartForRoleAsync("Admin", Username);

                // Update UI on main thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProfitChartSeries = series;
                    ProfitChartXAxes = xAxes;
                    ProfitChartYAxes = yAxes;

                    Console.WriteLine("Chart updated using service");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error preparing chart using service: {ex.Message}");
            }
        }
            
        private void InitializeChartProperties()
        {
            // Initialize all three charts with default data
            var (defaultSeries, defaultXAxes, defaultYAxes) = _chartService.InitializeDefaultChart();
    
            // Spending chart (red)
            SpendingChartSeries = defaultSeries;
            SpendingChartXAxes = defaultXAxes;
            SpendingChartYAxes = defaultYAxes;
    
            // Income chart (green) 
            IncomeChartSeries = defaultSeries;
            IncomeChartXAxes = defaultXAxes;
            IncomeChartYAxes = defaultYAxes;
    
            // Profit chart (blue)
            ProfitChartSeries = defaultSeries;
            ProfitChartXAxes = defaultXAxes; 
            ProfitChartYAxes = defaultYAxes;
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
                    _ = Task.Run(LoadAllThreeChartsAsync);
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
        
        // localization properties
        [ObservableProperty] private string _totalStockText = "";
        [ObservableProperty] private string _pendingText = "";
        [ObservableProperty] private string _lowStockText = "";

        [ObservableProperty] private string _warehouseOperationsText = "";
        [ObservableProperty] private string _logisticsOperationsText = "";
        [ObservableProperty] private string _quickActionsText = "";
        [ObservableProperty] private string _approvedText = "";
        [ObservableProperty] private string _availableText = "";
        
        [ObservableProperty] private string _warehouseOperationsHeaderText = "";
        [ObservableProperty] private string _logisticsOperationsHeaderText = "";
        [ObservableProperty] private string _quickActionsHeaderText = "";
        [ObservableProperty] private string _totalRequestsText = "";
        [ObservableProperty] private string _inInventoryText = "";
        [ObservableProperty] private string _readyForShipmentText = "";
        [ObservableProperty] private string _viewSystemReportsText = "";
        [ObservableProperty] private string _accessComprehensiveText = "";
        // Override the language change method
        protected override void OnLanguageChanged(object sender, EventArgs e)
        {
            FlowDirection = _localization.GetFlowDirection();
            UpdateLocalizedTexts();
            base.OnLanguageChanged(sender, e);
        }

        private void UpdateLocalizedTexts()
        {
            TotalStockText = Localize("TotalStock");
            PendingText = Localize("PendingRequests");
            LowStockText = Localize("LowStock");
            WarehouseOperationsText = Localize("WarehouseOperations");
            LogisticsOperationsText = Localize("LogisticsOperations");
            QuickActionsText = Localize("QuickActions");
            ApprovedText = Localize("Approved");
            AvailableText = Localize("Available");
            WarehouseOperationsHeaderText = $"📦 {Localize("WarehouseOperations")}";
            LogisticsOperationsHeaderText = $"🚚 {Localize("LogisticsOperations")}";
            QuickActionsHeaderText = $"⚡ {Localize("QuickActions")}";
            TotalRequestsText = Localize("TotalRequests");
            InInventoryText = Localize("InInventory");
            ReadyForShipmentText = Localize("ReadyForShipment");
            ViewSystemReportsText = Localize("ViewSystemReports");
            AccessComprehensiveText = Localize("AccessComprehensive");
        }
        
        [ObservableProperty] private FlowDirection _flowDirection = FlowDirection.LeftToRight;

        
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