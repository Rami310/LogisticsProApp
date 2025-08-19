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
using LogisticsPro.UI.Views.Logistics.LogisticsEmployee.Sections;

namespace LogisticsPro.UI.ViewModels
{
    public partial class LogisticsEmployeeDashboardViewModel : BaseDashboardViewModel
    {
        // ========================================
        // MAIN DATA COLLECTIONS
        // ========================================
        [ObservableProperty] private ObservableCollection<ProductRequest> _readyForShipmentOrders;
        [ObservableProperty] private ObservableCollection<ProductRequest> _recentDeliveries;
        [ObservableProperty] private ObservableCollection<ProductRequest> _myTasks;
        [ObservableProperty] private ObservableCollection<ProductRequest> _recentConfirmedOrders;
        [ObservableProperty] private ObservableCollection<ProductRequest> _recentAbortedOrders;
        
        // ========================================
        // UI STATE
        // ========================================
        [ObservableProperty] private string _currentSection = "Dashboard";
        [ObservableProperty] private Control? _currentSectionView;
        [ObservableProperty] private bool _isLoading;
        
        // ========================================
        // DELIVERY DIALOG PROPERTIES
        // ========================================
        [ObservableProperty] private bool _isDeliveryDialogOpen;
        [ObservableProperty] private ProductRequest? _selectedOrder;
        private string _deliveryNotes = "";
        [ObservableProperty] private bool _showAbortValidation;
        
        
        // ========================================
        // COMPUTED PROPERTIES WITH NOTIFICATIONS
        // ========================================
        public string WelcomeMessage => $"Welcome to Logistics Employee Dashboard, {Username}!";
        
        // For the sidebar profile
        public string UsernameInitial => Username?.Length > 0 ? Username[0].ToString().ToUpper() : "L";

        // Summary counts for dashboard cards
        public int PendingDeliveries => ReadyForShipmentOrders?.Count ?? 0;
        
        public int CompletedTasks => RecentDeliveries?
            .Where(r => r.RequestStatus == "Sold Out")
            .Count() ?? 0;
        
        public int TotalDelivered => RecentDeliveries?.Count ?? 0;
        
        
        // KPI Cards
        public int TotalTasks => (MyTasks?.Count ?? 0) + (CompletedTasks);
        public int ReadyForShipmentTasks => MyTasks?.Count(t => t.RequestStatus == "Ready for Shipment") ?? 0;
        
        // Show pagination controls only when there are multiple pages
        public bool ShowTaskPagination => TotalTaskPages > 1;
        
        // ========================================
        // PAGINATION PROPERTIES FOR MY TASKS
        // ========================================
        [ObservableProperty] private int _currentTaskPage = 1;
        [ObservableProperty] private int _itemsPerTaskPage = 16;  // 16 items per page
        [ObservableProperty] private int _totalTaskPages = 1;
        [ObservableProperty] private ObservableCollection<ProductRequest> _paginatedMyTasks = new();

        // Pagination button states
        public bool CanGoTaskPrevious => CurrentTaskPage > 1;
        public bool CanGoTaskNext => CurrentTaskPage < TotalTaskPages;
        
        
        // ========================================
        // CONSTRUCTOR
        // ========================================
        public LogisticsEmployeeDashboardViewModel(Action navigateToLogin, string username)
            : base(navigateToLogin, username, "Logistics Employee Dashboard")
        {
            // Initialize collections
            ReadyForShipmentOrders = new ObservableCollection<ProductRequest>();
            RecentDeliveries = new ObservableCollection<ProductRequest>();
            MyTasks = new ObservableCollection<ProductRequest>();
            RecentConfirmedOrders = new ObservableCollection<ProductRequest>();
            RecentAbortedOrders = new ObservableCollection<ProductRequest>();
            
            // Load dashboard data
            _ = Task.Run(async () => 
            {
                await Task.Delay(500);
                try 
                {
                    await LoadDashboardDataAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logistics employee dashboard loading failed: {ex.Message}");
                }
            });
        }

        // ========================================
        // NAVIGATION
        // ========================================
        [RelayCommand]
        private void NavigateToSection(string section)
        {
            CurrentSection = section;

            switch (section)
            {
                case "Dashboard":
                    CurrentSectionView = null;
                    break;
                case "MyTasks":
                    Console.WriteLine("Creating MyTasks section...");
                    var myTasksSection = new MyTasks();
                    Console.WriteLine("Setting DataContext...");
                    myTasksSection.DataContext = this;
                    Console.WriteLine("Setting CurrentSectionView...");
                    CurrentSectionView = myTasksSection;
                    Console.WriteLine("MyTasks section loaded");
                    break;
                case "Performance":
                    CurrentSectionView = null; // Future section
                    break;
                default:
                    CurrentSectionView = null;
                    break;
            }

            Console.WriteLine($"Logistics Employee navigated to {section}");
        }

        // ========================================
        // DATA LOADING
        // ========================================
        private async Task LoadDashboardDataAsync()
        {
            Console.WriteLine("Loading Logistics Employee dashboard data from API...");
            IsLoading = true;

            try
            {
                await ErrorHandler.TrySafeAsync("LoadLogisticsEmployeeDashboard", async () =>
                {
                    Console.WriteLine("Loading ready for shipment orders from API...");

                    var allRequests = await ProductRequestService.GetAllRequestsAsync();
                    var readyOrders = allRequests.Where(r => r.RequestStatus == "Ready for Shipment").ToList();
                    var deliveredOrders = allRequests.Where(r => r.RequestStatus == "Sold Out").ToList();
                    
                    Console.WriteLine($"Loaded {readyOrders.Count} ready orders, {deliveredOrders.Count} delivered orders from API");

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        // Clear collections first
                        ReadyForShipmentOrders.Clear();
                        RecentDeliveries.Clear();
                        MyTasks.Clear();
                        RecentConfirmedOrders.Clear();
                        RecentAbortedOrders.Clear();
                        
                        // Find items that were aborted via notes
                        var abortedByEmployee = allRequests
                            .Where(r => r.Notes?.Contains("ABORTED:") == true && 
                                        r.Notes?.Contains($"[Aborted by {Username}") == true)
                            .OrderByDescending(r => r.RequestDate)
                            .Take(4)
                            .ToList();

                        Console.WriteLine($"Found {abortedByEmployee.Count} orders aborted by {Username}");

                        // Add recent aborted orders (last 4 for activity summary)
                        foreach (var aborted in abortedByEmployee)
                        {
                            RecentAbortedOrders.Add(aborted);
                        }

                        
                        // Sort and assign DisplayId BEFORE adding to collections
                        var sortedReadyOrders = readyOrders
                            .OrderByDescending(r => r.RequestDate)
                            .ThenByDescending(r => r.Id)
                            .ToList();

                        // Assign sequential display IDs (1, 2, 3...)
                        for (int i = 0; i < sortedReadyOrders.Count; i++)
                        {
                            sortedReadyOrders[i].DisplayId = i + 1;
                        }

                        // Add to collections ONCE with proper DisplayId
                        foreach (var order in sortedReadyOrders)
                        {
                            ReadyForShipmentOrders.Add(order);
                            MyTasks.Add(order);
                        }

                        // Add recent deliveries (last 30 days)
                        var recentDeliveries = deliveredOrders
                            .Where(r => r.RequestDate >= DateTime.Now.AddDays(-30))
                            .OrderByDescending(r => r.RequestDate)
                            .ToList();

                        foreach (var delivery in recentDeliveries)
                        {
                            RecentDeliveries.Add(delivery);
                        }

                        // Add recent confirmed orders (last 4 for activity summary)
                        foreach (var confirmed in deliveredOrders.OrderByDescending(r => r.RequestDate).Take(4))
                        {
                            RecentConfirmedOrders.Add(confirmed);
                        }

                        // Notify UI of count changes
                        OnPropertyChanged(nameof(PendingDeliveries));
                        OnPropertyChanged(nameof(CompletedTasks));
                        OnPropertyChanged(nameof(TotalDelivered));
                        OnPropertyChanged(nameof(TotalTasks));
                        OnPropertyChanged(nameof(ReadyForShipmentTasks));
                        
                        // Update pagination after loading data
                        UpdateTasksPagination();
                        
                        Console.WriteLine($"Logistics employee dashboard updated - {ReadyForShipmentOrders.Count} pending deliveries, {RecentDeliveries.Count} recent deliveries");
                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadDashboardDataAsync error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                Console.WriteLine("Logistics employee dashboard data loading completed");
            }
        }
        
        
        // ========================================
        // UTILITY COMMANDS
        // ========================================
        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            Console.WriteLine("Logistics Employee refreshing all data...");
            await LoadDashboardDataAsync();
        }
        
        // ========================================
        // DELIVERY DIALOG COMMANDS
        // ========================================
        [RelayCommand]
        private void OpenDeliveryDialog(ProductRequest order)
        {
            if (order == null) return;

            SelectedOrder = order;
            DeliveryNotes = "";
            ShowAbortValidation = false;
            IsDeliveryDialogOpen = true;

            Console.WriteLine($"Opening delivery dialog for order {order.Id} - {order.Product?.Name}");
        }

        [RelayCommand]
        private void CloseDeliveryDialog()
        {
            IsDeliveryDialogOpen = false;
            SelectedOrder = null;
            DeliveryNotes = "";
            ShowAbortValidation = false;

            Console.WriteLine("Delivery dialog closed");
        }

        [RelayCommand]
        private async Task ConfirmDeliveryFromDialogAsync()
        {
            if (SelectedOrder == null || SelectedOrder.RequestStatus != "Ready for Shipment")
            {
                Console.WriteLine("Cannot confirm delivery: Invalid or not ready for shipment");
                return;
            }

            Console.WriteLine($"Confirming delivery for order {SelectedOrder.Id} - {SelectedOrder.Product?.Name}");

            try
            {
                // Use the existing service method
                var success = await ProductRequestService.ConfirmDeliveryAsync(SelectedOrder.Id, Username);

                if (success)
                {
                    Console.WriteLine(
                        $"Order {SelectedOrder.Id} delivered successfully - Status updated to 'Sold Out', inventory updated");

                    // Close dialog and reload data
                    CloseDeliveryDialog();
                    await LoadDashboardDataAsync();
                }
                else
                {
                    Console.WriteLine($"Failed to confirm delivery for order {SelectedOrder.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error confirming delivery: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AbortDeliveryFromDialogAsync()
        {
            if (SelectedOrder == null || SelectedOrder.RequestStatus != "Ready for Shipment")
            {
                Console.WriteLine("Cannot abort delivery: Invalid or not ready for shipment");
                return;
            }

            if (string.IsNullOrWhiteSpace(DeliveryNotes))
            {
                ShowAbortValidation = true;
                Console.WriteLine("Abort reason is required");
                return;
            }

            ShowAbortValidation = false;
            Console.WriteLine($"Aborting delivery for order {SelectedOrder.Id} - {SelectedOrder.Product?.Name}");

            try
            {
                // Return to "Approved" status + Add abort tracking in notes
                var success = await ProductRequestService.UpdateRequestStatusAsync(
                    SelectedOrder.Id,
                    "Approved", // Returns to inventory (available for manager to re-mark)
                    Username,
                    $"ABORTED: {DeliveryNotes} [Aborted by {Username} on {DateTime.Now:MM/dd HH:mm}]"
                );

                if (success)
                {
                    Console.WriteLine($"Order {SelectedOrder.Id} delivery aborted - Item returned to approved inventory");
                    Console.WriteLine($"No money movement - item was already paid for");

                    CloseDeliveryDialog();
                    await LoadDashboardDataAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error aborting delivery: {ex.Message}");
            }
        }
        
        public string DeliveryNotes
        {
            get => _deliveryNotes;
            set
            {
                if (SetProperty(ref _deliveryNotes, value))
                {
                    // Hide validation message when user starts typing
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        ShowAbortValidation = false;
                    }
                }
            }
        }
        
        // ========================================
        // PAGINATION COMMANDS
        // ========================================
        [RelayCommand]
        private void NextTaskPage()
        {
            if (CanGoTaskNext)
            {
                CurrentTaskPage++;
                UpdateTasksPagination();
            }
        }

        [RelayCommand]
        private void PreviousTaskPage()
        {
            if (CanGoTaskPrevious)
            {
                CurrentTaskPage--;
                UpdateTasksPagination();
            }
        }
        
        /// <summary>
        /// Update pagination for MyTasks section (16 items per page)
        /// </summary>
        private void UpdateTasksPagination()
        {
            if (MyTasks == null || MyTasks.Count == 0)
            {
                TotalTaskPages = 1;
                CurrentTaskPage = 1;
                PaginatedMyTasks.Clear();
                OnPropertyChanged(nameof(CanGoTaskPrevious));
                OnPropertyChanged(nameof(CanGoTaskNext));
                OnPropertyChanged(nameof(ShowTaskPagination)); // Notify pagination visibility
                return;
            }

            // Calculate total pages
            TotalTaskPages = (int)Math.Ceiling((double)MyTasks.Count / ItemsPerTaskPage);

            // Ensure current page is valid
            if (CurrentTaskPage > TotalTaskPages) CurrentTaskPage = TotalTaskPages;
            if (CurrentTaskPage < 1) CurrentTaskPage = 1;

            // Calculate items for current page
            var startIndex = (CurrentTaskPage - 1) * ItemsPerTaskPage;
            var itemsToTake = Math.Min(ItemsPerTaskPage, MyTasks.Count - startIndex);

            // Update paginated collection
            PaginatedMyTasks.Clear();
            var pageItems = MyTasks.Skip(startIndex).Take(itemsToTake).ToList();

            foreach (var task in pageItems)
            {
                PaginatedMyTasks.Add(task);
            }

            // Notify pagination button states
            OnPropertyChanged(nameof(CanGoTaskPrevious));
            OnPropertyChanged(nameof(CanGoTaskNext));
            OnPropertyChanged(nameof(ShowTaskPagination)); // Notify pagination visibility

            Console.WriteLine($"Tasks pagination updated - Page {CurrentTaskPage}/{TotalTaskPages}, showing {PaginatedMyTasks.Count} items");
        }
        
        // helper method to extract employee names from notes
        public string GetEmployeeNameFromNotes(string notes)
        {
            if (string.IsNullOrEmpty(notes)) return "Employee";
    
            // Extract from "ABORTED: reason [Aborted by username on date]"
            if (notes.Contains("[Aborted by ") || notes.Contains("[Confirmed by "))
            {
                var startIndex = notes.IndexOf(" by ") + 4;
                if (startIndex > 3)
                {
                    var endIndex = notes.IndexOf(" on ", startIndex);
                    if (endIndex > startIndex)
                    {
                        return notes.Substring(startIndex, endIndex - startIndex);
                    }
                }
            }
    
            // Fallback: look for "by username" pattern
            var byIndex = notes.LastIndexOf(" by ");
            if (byIndex >= 0)
            {
                var nameStart = byIndex + 4;
                var remaining = notes.Substring(nameStart);
                var spaceIndex = remaining.IndexOfAny(new[] { ' ', ']', '\n' });
                return spaceIndex > 0 ? remaining.Substring(0, spaceIndex) : remaining;
            }
    
            return "Employee"; // Default fallback
        }
        
    }
}