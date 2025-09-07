using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogisticsPro.UI.Services;
using LogisticsPro.UI.ViewModels;
namespace LogisticsPro.UI.ViewModels.Shared
{
    public partial class BaseRevenueViewModel : LocalizedViewModelBase
    {
        [ObservableProperty]
        private decimal _currentRevenue;
        
        [ObservableProperty]
        private decimal _availableBudget;
        
        [ObservableProperty]
        private decimal _totalSpent;
        
        [ObservableProperty]
        private decimal _monthlySpent;
        
        [ObservableProperty]
        private double _budgetUtilizationPercentage;
        
        [ObservableProperty]
        private DateTime _lastUpdated;
        
        [ObservableProperty]
        private bool _isLoading;
        
        [ObservableProperty]
        private string _userRole;

        [ObservableProperty]
        private int _totalOrders;
        
        [ObservableProperty]
        private int _approvedOrders;
        
        [ObservableProperty]
        private int _pendingOrders;
        
        [ObservableProperty]
        private double _orderApprovalPercentage;
        
        [ObservableProperty]
        private string _approvedOrdersText = "No orders yet";

        [ObservableProperty]
        private double _budgetUtilizationProgress;

        [ObservableProperty]
        private string _budgetUtilizationText = "";

        //localization properties
        [ObservableProperty] private string _financialOverviewText = "";
        [ObservableProperty] private string _lastUpdatedText = "";
        [ObservableProperty] private string _availableBudgetText = "";
        [ObservableProperty] private string _readyToSpendText = "";
        [ObservableProperty] private string _totalSpentText = "";
        [ObservableProperty] private string _allTimeSpendingText = "";
        [ObservableProperty] private string _thisMonthText = "";
        [ObservableProperty] private string _monthlySpendingText = "";
        [ObservableProperty] private string _orderProgressText = "";
        [ObservableProperty] private string _budgetUtilizationDetailsText = "";

        // Color properties for progress bars
        partial void OnBudgetUtilizationProgressChanged(double value)
        {
            OnPropertyChanged(nameof(BudgetUtilizationColor));
        }
        
        public string BudgetUtilizationColor => BudgetUtilizationProgress switch
        {
            < 50 => "#27AE60",  // Green - Good utilization
            < 75 => "#F39C12",  // Orange - Moderate utilization
            < 90 => "#E67E22",  // Dark Orange - High utilization
            _ => "#E74C3C"       // Red - Very high utilization
        };

        partial void OnOrderApprovalPercentageChanged(double value)
        {
            OnPropertyChanged(nameof(OrderProgressColor));
        }

        public string OrderProgressColor => OrderApprovalPercentage switch
        {
            < 30 => "#E74C3C",  // Red - Low approval rate
            < 70 => "#F39C12",  // Orange - Medium approval rate  
            _ => "#27AE60"       // Green - High approval rate
        };
        
        public BaseRevenueViewModel(string userRole): base()
        {
            UserRole = userRole;
            UpdateLocalizedTexts();
            _ = Task.Run(LoadRevenueDataAsync);
        }

        protected override void OnLanguageChanged(object sender, EventArgs e)
        {
            UpdateLocalizedTexts();
            base.OnLanguageChanged(sender, e);
        }
        
        private void UpdateLocalizedTexts()
        {
            FinancialOverviewText = Localize("AdministratorFinancialOverview");
            LastUpdatedText = LastUpdated != default 
                ? $"{Localize("LastUpdated")}: {LastUpdated:MMM dd, yyyy HH:mm}"
                : Localize("LastUpdated");
            AvailableBudgetText = Localize("AvailableBudget");
            ReadyToSpendText = Localize("ReadyToSpend");
            TotalSpentText = Localize("TotalSpent");
            AllTimeSpendingText = Localize("AllTimeSpending");
            ThisMonthText = Localize("ThisMonth");
            MonthlySpendingText = Localize("MonthlySpending");
            OrderProgressText = $"📋 {Localize("OrderProgress")}";
            
            // Update budget utilization text with localization
            var totalBudget = AvailableBudget + TotalSpent;
            BudgetUtilizationDetailsText = $"{Localize("Used")} ${TotalSpent:N0} {Localize("Of")} ${totalBudget:N0} {Localize("TotalBudget")}";
        }

        
        
        /// <summary>
        /// Load revenue data from API (your original working method)
        /// </summary>
        public async Task LoadRevenueDataAsync()
        {
            IsLoading = true;
            
            try
            {
                var revenue = await RevenueService.GetCurrentRevenueAsync();
                var monthlyData = await RevenueService.GetMonthlySpendingAsync();
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    CurrentRevenue = revenue.CurrentRevenue;
                    AvailableBudget = revenue.AvailableBudget;
                    TotalSpent = revenue.TotalSpent;
                    MonthlySpent = monthlyData.CurrentMonthSpent;
                    LastUpdated = revenue.LastUpdated;
                    
                    // Calculate budget utilization (your version)
                    BudgetUtilizationPercentage = CurrentRevenue > 0 
                        ? Math.Min((double)(TotalSpent / CurrentRevenue) * 100, 100) 
                        : 0;

                    // Calculate monthly budget progress
                    UpdateBudgetUtilizationProgress();
                        
                    Console.WriteLine($"💰 Revenue loaded for {UserRole} - Available: ${AvailableBudget:N0}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to load revenue data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Update order statistics and progress bar (called from WarehouseManagerDashboardViewModel)
        /// </summary>
        public void UpdateOrderProgress(int totalOrders, int approvedOrders, int pendingOrders)
        {
            TotalOrders = totalOrders;
            ApprovedOrders = approvedOrders;
            PendingOrders = pendingOrders;
            
            // Calculate order approval percentage
            OrderApprovalPercentage = TotalOrders > 0 
                ? (double)ApprovedOrders / TotalOrders * 100 
                : 0;
                
            // Role-specific display text
            ApprovedOrdersText = UserRole switch
            {
                "Logistics Manager" => TotalOrders > 0 
                    ? $"{ApprovedOrders} of {TotalOrders} items shipped ({OrderApprovalPercentage:F0}%)"
                    : "No shipments yet",
                _ => TotalOrders > 0 
                    ? $"{ApprovedOrders} of {TotalOrders} orders approved ({OrderApprovalPercentage:F0}%)"
                    : "No orders yet"
            };
        
            Console.WriteLine($"📊 {UserRole} Progress: {ApprovedOrders}/{TotalOrders} = {OrderApprovalPercentage:F1}%");
                
            Console.WriteLine($"📊 Order Progress: {ApprovedOrders}/{TotalOrders} = {OrderApprovalPercentage:F1}%");
        }

        private void UpdateBudgetUtilizationProgress()
        {
            var totalBudget = AvailableBudget + TotalSpent;
    
            // Calculate what percentage of total budget has been used
            BudgetUtilizationProgress = totalBudget > 0 
                ? Math.Min((double)(TotalSpent / totalBudget * 100), 100)
                : 0;
        
            // Update with localized text
            BudgetUtilizationDetailsText = $"{Localize("Used")} ${TotalSpent:N0} {Localize("Of")} ${totalBudget:N0} {Localize("TotalBudget")}";
    
            Console.WriteLine($"📊 Budget Utilization: ${TotalSpent:N0}/${totalBudget:N0} = {BudgetUtilizationProgress:F1}%");
        }

        [RelayCommand]
        public async Task RefreshRevenue()
        {
            await LoadRevenueDataAsync();
        }

        [RelayCommand]
        public void ViewDetailedFinancials()
        {
            Console.WriteLine($"{UserRole} viewing detailed financials");
        }

        /// <summary>
        /// Check if user has budget for a purchase
        /// </summary>
        public bool CanAfford(decimal amount)
        {
            return amount <= AvailableBudget;
        }
    }
}