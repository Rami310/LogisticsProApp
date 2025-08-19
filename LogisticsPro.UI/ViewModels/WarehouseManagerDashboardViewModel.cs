using System;
using System.Collections.Generic;
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
using LogisticsPro.UI.Views.Warehouse.WarehouseManager.Sections;

namespace LogisticsPro.UI.ViewModels
{
    public partial class WarehouseManagerDashboardViewModel : BaseDashboardViewModel
    {
        // ========================================
        // EXISTING PROPERTIES
        // ========================================
        [ObservableProperty] private ObservableCollection<InventoryItem> _inventoryItems;

        [ObservableProperty] private ObservableCollection<ProductRequest> _pendingRequests;

        [ObservableProperty] private ObservableCollection<InventoryItem> _lowStockItems;

        [ObservableProperty] private bool _isLoading;

        [ObservableProperty] private bool _isRequestDialogOpen;

        [ObservableProperty] private ProductRequest _newRequest = new ProductRequest();

        [ObservableProperty] private Product? _selectedProduct;

        [ObservableProperty] private ObservableCollection<Product> _products;

        [ObservableProperty] private string _currentSection = "Dashboard";

        [ObservableProperty] private Control? _currentSectionView;

        [ObservableProperty] private BaseRevenueViewModel _revenueViewModel;

        private List<InventoryItem> _allInventoryItems = new List<InventoryItem>();
        
        public string WelcomeMessage => $"Welcome to Warehouse Dashboard, {Username}!";


        // ========================================
        // NEW PROPERTIES FOR PRODUCT REQUESTS SECTION
        // ========================================
        [ObservableProperty] private ObservableCollection<ProductRequest> _allRequests;

        [ObservableProperty] private int _approvedRequestsCount;

        [ObservableProperty] private int _totalRequestsThisMonth;

        [ObservableProperty] private decimal _totalSpentOnRequests;

        [ObservableProperty] private decimal _totalCost;

        [ObservableProperty] private decimal _budgetAfterOrder;


        [ObservableProperty] private int _currentRequestPage = 1;

        [ObservableProperty] private int _itemsPerRequestPage = 10;

        [ObservableProperty] private int _totalRequestPages = 1;

        [ObservableProperty] private ObservableCollection<ProductRequest> _paginatedRequests = new();
        
        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = "";
        }
        public bool CanGoPrevious => CurrentRequestPage > 1;
        public bool CanGoNext => CurrentRequestPage < TotalRequestPages;

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
                    CurrentSectionView = null; // Show main dashboard
                    break;

                case "InventoryCheck":
                    CurrentSectionView = new InventoryManagementSection
                    {
                        DataContext = this
                    };
                    break;

                case "ProductRequests":
                    CurrentSectionView = new ProductRequestsSection
                    {
                        DataContext = this
                    };
                    break;

                case "StockTransfers":
                case "LowStock":
                    CurrentSectionView = new InventoryManagementSection
                    {
                        DataContext = this
                    };
                    break;

                default:
                    CurrentSectionView = null;
                    break;
            }

            Console.WriteLine(
                $"Navigated to {section} - CurrentSectionView: {CurrentSectionView?.GetType().Name ?? "Dashboard"}");
        }

        // ========================================
        // CONSTRUCTOR
        // ========================================
        public WarehouseManagerDashboardViewModel(Action navigateToLogin, string username)
            : base(navigateToLogin, username, "Warehouse Dashboard")
        {
            // Initialize revenue section
            RevenueViewModel = new BaseRevenueViewModel("Warehouse Manager");

            // Initialize ObservableCollections
            InventoryItems = new ObservableCollection<InventoryItem>();
            PendingRequests = new ObservableCollection<ProductRequest>();
            LowStockItems = new ObservableCollection<InventoryItem>();
            Products = new ObservableCollection<Product>();
            AllRequests = new ObservableCollection<ProductRequest>();

            // Initialize NewRequest with property change monitoring
            NewRequest = new ProductRequest
            {
                RequestedBy = username,
                RequestDate = DateTime.Now,
                RequestedQuantity = 1
            };

            // Load data
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                try
                {
                    await LoadDashboardDataAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Dashboard loading failed: {ex.Message}");
                }
            });
        }

        // ========================================
        // DATA LOADING
        // ========================================
        private async Task LoadDashboardDataAsync()
        {
            Console.WriteLine("LoadDashboardDataAsync started");
            IsLoading = true;

            try
            {
                await ErrorHandler.TrySafeAsync("LoadWarehouseDashboard", async () =>
                {
                    Console.WriteLine("Loading warehouse dashboard data...");

                    // Get all inventory items (ASYNC)
                    var inventory = await InventoryService.GetAllInventoryAsync();
                    Console.WriteLine($"Loaded {inventory.Count} inventory items");

                    // Get pending requests (ASYNC)
                    var requests = await ProductRequestService.GetRequestsByStatusAsync("Pending");
                    Console.WriteLine($"Loaded {requests.Count} pending requests");

                    // Calculate low stock items with threshold of 10
                    var lowStock = inventory.Where(item => item.QuantityInStock < 10).ToList();
                    Console.WriteLine($"Found {lowStock.Count} low stock items (threshold: 10)");

                    // Get all products (ASYNC)
                    var products = await ProductService.GetAllProductsAsync();
                    Console.WriteLine($"Loaded {products.Count} products");

                    // Load all requests for the requests section
                    await LoadAllRequestsAsync();

                    // Update collections ON UI THREAD
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Console.WriteLine("Updating UI collections...");

                        // Clear and populate inventory
                        _allInventoryItems = inventory.ToList();
                        InventoryItems.Clear();
                        foreach (var item in inventory)
                        {
                            InventoryItems.Add(item);
                        }

                        // Clear and populate pending requests
                        PendingRequests.Clear();
                        foreach (var request in requests)
                        {
                            PendingRequests.Add(request);
                        }

                        // Clear and populate low stock items with real data
                        LowStockItems.Clear();
                        foreach (var item in lowStock)
                        {
                            LowStockItems.Add(item);
                        }

                        // Clear and populate products
                        Products.Clear();
                        foreach (var product in products)
                        {
                            Products.Add(product);
                        }

                        Console.WriteLine(
                            $"UI Updated - Inventory: {InventoryItems.Count}, Requests: {PendingRequests.Count}, Low Stock: {LowStockItems.Count}, Products: {Products.Count}");

                        // Debug low stock items
                        foreach (var item in LowStockItems)
                        {
                            Console.WriteLine($"Low Stock: {item.Product?.Name} - Stock: {item.QuantityInStock}");
                        }
                        OnPropertyChanged(nameof(TotalStockQuantity));
                        OnPropertyChanged(nameof(AvailableBudgetDisplayK));

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
                Console.WriteLine("LoadDashboardDataAsync completed");
            }
        }

        private async Task LoadAllRequestsAsync()
        {
            try
            {
                // Get all requests for this manager
                var allRequests = await ProductRequestService.GetRequestsByUserAsync(Username);

                // Update UI on main thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Sort by newest first (by RequestDate, then by database ID)
                    var sortedRequests = allRequests
                        .OrderByDescending(r => r.RequestDate) // Newest date first
                        .ThenByDescending(r => r.Id) // Then highest database ID first
                        .ToList();

                    // Assign UI Display IDs (1, 2, 3...) to the sorted list
                    for (int i = 0; i < sortedRequests.Count; i++)
                    {
                        sortedRequests[i].DisplayId = i + 1; // Display ID starts from 1 for newest
                    }

                    AllRequests = new ObservableCollection<ProductRequest>(sortedRequests);

                    // Update statistics (using original data, not display IDs)
                    ApprovedRequestsCount = allRequests.Count(r => r.RequestStatus == "Approved");
                    TotalRequestsThisMonth = allRequests.Count(r => r.RequestDate.Month == DateTime.Now.Month);
                    TotalSpentOnRequests = allRequests
                        .Where(r => r.RequestStatus != "Rejected" && r.RequestStatus != "Cancelled")
                        .Sum(r => r.TotalCost);

                    // Update pagination after loading data
                    UpdateRequestsPagination();

                    // Update revenue section with order progress
                    UpdateRevenueOrderProgress();
                });

                Console.WriteLine(
                    $"Loaded {allRequests.Count} total requests for {Username} (newest = Display ID 1)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load all requests: {ex.Message}");

                // Fallback to empty collection
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    AllRequests = new ObservableCollection<ProductRequest>();
                    ApprovedRequestsCount = 0;
                    TotalRequestsThisMonth = 0;
                    TotalSpentOnRequests = 0;
                    UpdateRequestsPagination();
                    UpdateRevenueOrderProgress();
                });
            }
        }

        // ========================================
        // EXISTING COMMANDS (from your working version)
        // ========================================

        [RelayCommand]
        private void FilterInventory()
        {
            Console.WriteLine($"Real-time search triggered with text: '{SearchText}'");

            ErrorHandler.TrySafe("FilterInventory", () =>
            {
                var sourceData = _allInventoryItems.Any() ? _allInventoryItems : InventoryItems.ToList();
                InventoryItems.Clear();
                var searchLower = SearchText?.ToLower();

                var filtered = string.IsNullOrWhiteSpace(searchLower)
                    ? sourceData
                    : sourceData.Where(i =>
                        i.Product?.Name?.ToLower().Contains(searchLower) == true ||
                        i.Product?.SKU?.ToLower().Contains(searchLower) == true ||
                        i.Product?.Category?.ToLower().Contains(searchLower) == true ||
                        i.Location?.ToLower().Contains(searchLower) == true);

                foreach (var item in filtered)
                {
                    InventoryItems.Add(item);
                    Console.WriteLine($"Added to filtered results: {item.Product?.Name}");
                }

                Console.WriteLine($"Filtered {InventoryItems.Count} items from {sourceData.Count} total");
            });
        }

        [RelayCommand]
        private void OpenRequestDialog()
        {
            // Create fresh request instance
                NewRequest = new ProductRequest
                {
                    RequestedBy = Username,
                    RequestDate = DateTime.Now,
                    RequestedQuantity = 1
                };

            // Reset selections and text
            SelectedProduct = null;
            QuantityText = "1"; // Reset the text binding
            TotalCost = 0;
            BudgetAfterOrder = RevenueViewModel.AvailableBudget;

            IsRequestDialogOpen = true;
        }

        [RelayCommand]
        private void CloseRequestDialog()
        {
            IsRequestDialogOpen = false;
        }

        [RelayCommand]
        private async Task SubmitRequestAsync()
        {
            if (SelectedProduct == null || NewRequest.RequestedQuantity <= 0)
            {
                Console.WriteLine("Cannot submit request: Invalid product or quantity");
                return;
            }

            // Check if enough budget available (TotalCost is already calculated)
            if (!RevenueViewModel.CanAfford(TotalCost))
            {
                Console.WriteLine(
                    $"Insufficient budget - Order: ${TotalCost:F2}, Available: ${RevenueViewModel.AvailableBudget:F2}");
                return;
            }

            await ErrorHandler.TrySafeAsync("SubmitProductRequest", async () =>
            {
                // Set product ID (TotalCost is already set in UpdateCostCalculation)
                NewRequest.ProductId = SelectedProduct.Id;

                // Add request using ASYNC method
                var success = await ProductRequestService.AddRequestAsync(NewRequest);

                if (success)
                {
                    // Deduct money from company revenue using the calculated TotalCost
                    var revenueDeducted = await RevenueService.DeductForOrderAsync(
                        NewRequest.Id,
                        TotalCost,
                        Username
                    );

                    if (revenueDeducted)
                    {
                        Console.WriteLine($"Request submitted and ${TotalCost:F2} deducted from revenue");

                        // Refresh revenue display
                        await RevenueViewModel.LoadRevenueDataAsync();
                    }
                    else
                    {
                        Console.WriteLine("Request submitted but revenue deduction failed");
                    }

                    // Close dialog
                    IsRequestDialogOpen = false;

                    // Reload data to show new request
                    await LoadDashboardDataAsync();
                }
                else
                {
                    Console.WriteLine("Failed to submit request");
                }
            });
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            await LoadDashboardDataAsync();
        }

        // ========================================
        // NEW COMMANDS FOR PRODUCT REQUESTS SECTION
        // ========================================
        [RelayCommand]
        private void FilterRequests()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Reload all requests
                _ = Task.Run(LoadAllRequestsAsync);
                return;
            }

            var search = SearchText.ToLower();

            var allUserRequests = ProductRequestService.GetRequestsByUser(Username);
            var filtered = allUserRequests.Where(r =>
                    r.Product.Name.ToLower().Contains(search) ||
                    r.Product.SKU.ToLower().Contains(search) ||
                    r.RequestStatus.ToLower().Contains(search) ||
                    r.Notes?.ToLower().Contains(search) == true)
                .ToList();

            AllRequests = new ObservableCollection<ProductRequest>(filtered);
        }

        [RelayCommand]
        public async Task CancelRequestAsync(ProductRequest request)
        {
            if (request == null || request.RequestStatus != "Pending")
            {
                Console.WriteLine("Cannot cancel non-pending request");
                return;
            }

            try
            {
                Console.WriteLine(
                    $"Starting cancel for request {request.Id} - Current status: {request.RequestStatus}");

                // Update request status
                await ProductRequestService.CancelRequestAsync(request.Id, Username);
                Console.WriteLine($"Status updated in database for request {request.Id}");

                // Restore revenue manually
                var restored =
                    await RevenueService.RestoreForCancelledOrderAsync(request.Id, request.TotalCost, Username);

                if (restored)
                {
                    Console.WriteLine($"Request {request.Id} cancelled and revenue restored");

                    // Add a small delay to ensure database update completes
                    await Task.Delay(500);

                    // Update the specific item in the collection FIRST
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        var itemToUpdate = AllRequests.FirstOrDefault(r => r.Id == request.Id);
                        if (itemToUpdate != null)
                        {
                            itemToUpdate.RequestStatus = "Cancelled";
                            Console.WriteLine($"Updated item {request.Id} status to Cancelled in UI collection");
                        }
                    });

                    // Then refresh all data
                    await LoadAllRequestsAsync();
                    await RevenueViewModel.LoadRevenueDataAsync();
                    await LoadDashboardDataAsync();
                    UpdateRevenueOrderProgress();

                    Console.WriteLine($"Data refresh completed");
                }
                else
                {
                    Console.WriteLine($"Request cancelled but revenue restoration failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling request: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        [RelayCommand]
        private void ViewRequest(ProductRequest request)
        {
            // Future: Open request details dialog
            Console.WriteLine($"Viewing request {request.Id} - {request.Product.Name}");
        }

        // ========================================
        // COST ESTIMATION METHODS
        // ========================================
        partial void OnSelectedProductChanged(Product? value)
        {
            UpdateCostCalculation();
        }

        private void UpdateCostCalculation()
        {
            if (SelectedProduct != null && NewRequest != null && NewRequest.RequestedQuantity > 0)
            {
                var newTotalCost = SelectedProduct.UnitPrice * NewRequest.RequestedQuantity;
                TotalCost = newTotalCost;
                NewRequest.TotalCost = TotalCost;
                BudgetAfterOrder = RevenueViewModel.AvailableBudget - TotalCost;

                Console.WriteLine($"Cost calculated: ${TotalCost:F2}");
            }
            else
            {
                TotalCost = 0;
                if (NewRequest != null)
                {
                    NewRequest.TotalCost = 0;
                }
                BudgetAfterOrder = RevenueViewModel.AvailableBudget;
            }
        }

        private string _searchText = "";
        private bool _isFiltering = false;

        public string SearchText
        {
            get => _searchText;
            set
            {
                Console.WriteLine($"SearchText setter called with: '{value}'"); // This line works
        
                if (SetProperty(ref _searchText, value) && !_isFiltering)
                {
                    _isFiltering = true;
                    try
                    {
                        Console.WriteLine($"About to call FilterInventory with: '{value}'");
                        FilterInventory();
                        Console.WriteLine($"FilterInventory completed");
                    }
                    finally
                    {
                        _isFiltering = false;
                    }
                }
                else
                {
                    Console.WriteLine($"FilterInventory NOT called - isFiltering: {_isFiltering}"); // ADD THIS
                }
            }
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CanGoNext)
            {
                CurrentRequestPage++;
                UpdateRequestsPagination();
            }
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (CanGoPrevious)
            {
                CurrentRequestPage--;
                UpdateRequestsPagination();
            }
        }

        // Update pagination method
        private void UpdateRequestsPagination()
        {
            if (AllRequests == null || AllRequests.Count == 0)
            {
                TotalRequestPages = 1;
                CurrentRequestPage = 1;
                PaginatedRequests.Clear();
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));
                return;
            }

            // Calculate total pages
            TotalRequestPages = (int)Math.Ceiling((double)AllRequests.Count / ItemsPerRequestPage);

            // Ensure current page is valid
            if (CurrentRequestPage > TotalRequestPages) CurrentRequestPage = TotalRequestPages;
            if (CurrentRequestPage < 1) CurrentRequestPage = 1;

            // Calculate items for current page
            var startIndex = (CurrentRequestPage - 1) * ItemsPerRequestPage;
            var itemsToTake = Math.Min(ItemsPerRequestPage, AllRequests.Count - startIndex);

            // Just take the items (DisplayId already set correctly)
            PaginatedRequests.Clear();
            var pageItems = AllRequests.Skip(startIndex).Take(itemsToTake).ToList();

            foreach (var request in pageItems)
            {
                PaginatedRequests.Add(request);  // DisplayId already correct (1, 2, 3...)
            }

            // Notify pagination button states
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));

            Console.WriteLine($"Pagination updated - Page {CurrentRequestPage}/{TotalRequestPages}, showing {PaginatedRequests.Count} items");
            Console.WriteLine($"   Showing Display IDs: {string.Join(", ", pageItems.Select(r => r.DisplayId))}");
        }
        
        /// <summary>
        /// Update revenue section with order progress information
        /// </summary>
        private void UpdateRevenueOrderProgress()
        {
            if (RevenueViewModel != null && AllRequests != null)
            {
                // Calculate order statistics
                var totalOrders = AllRequests.Count;
                var approvedOrders = AllRequests.Count(r => r.RequestStatus == "Approved");
                var pendingOrders = AllRequests.Count(r => r.RequestStatus == "Pending");
        
                // Update revenue view model with order progress
                RevenueViewModel.UpdateOrderProgress(totalOrders, approvedOrders, pendingOrders);
        
                Console.WriteLine($"Updated revenue progress - Total: {totalOrders}, Approved: {approvedOrders}, Pending: {pendingOrders}");
            }
        }
        
        private string _quantityText = "1";
        public string QuantityText
        {
            get => _quantityText;
            set
            {
                if (SetProperty(ref _quantityText, value))
                {
                    // Convert to int and update NewRequest.RequestedQuantity
                    if (int.TryParse(value, out int quantity) && quantity > 0)
                    {
                        if (NewRequest != null)
                        {
                            NewRequest.RequestedQuantity = quantity;
                            UpdateCostCalculation();
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        // Allow empty for better UX - don't show error immediately
                        if (NewRequest != null)
                        {
                            NewRequest.RequestedQuantity = 1; // Default to 1
                            UpdateCostCalculation();
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets the total quantity of all inventory items (sum of all stock)
        /// </summary>
        public int TotalStockQuantity
        {
            get
            {
                if (InventoryItems == null || !InventoryItems.Any())
                    return 0;
            
                return InventoryItems.Sum(item => item.QuantityInStock);
            }
        }
        
        // <summary>
        /// Gets the available budget formatted as "901K" style
        /// </summary>
        public string AvailableBudgetDisplayK
        {
            get
            {
                if (RevenueViewModel?.AvailableBudget == null)
                    return "$0K";
            
                var budget = RevenueViewModel.AvailableBudget;
                var budgetInK = Math.Round(budget / 1000, 0);
                return $"${budgetInK:F0}K";
            }
        }
    }
}