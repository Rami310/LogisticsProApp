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
using LogisticsPro.UI.Views.Logistics.LogisticsManager.Sections;

namespace LogisticsPro.UI.ViewModels
{
    public partial class LogisticsManagerDashboardViewModel : BaseDashboardViewModel
    {
        // Summary counts for processed orders
        public int RejectedShipmentCount => ShipmentRequests?.Count(r => r.RequestStatus == "Rejected") ?? 0;
        public int CancelledShipmentCount => ShipmentRequests?.Count(r => r.RequestStatus == "Cancelled") ?? 0;

        // ========================================
        // MAIN DATA COLLECTIONS
        // ========================================
        [ObservableProperty] private ObservableCollection<ProductRequest> _productRequests;
        [ObservableProperty] private ObservableCollection<ProductRequest> _shipmentRequests;
        
        // ========================================
        // UI STATE
        // ========================================
        [ObservableProperty] private string _currentSection = "Dashboard";
        [ObservableProperty] private Control? _currentSectionView;
        [ObservableProperty] private bool _isLoading;
        
        // ========================================
        // COMPUTED PROPERTIES WITH NOTIFICATIONS
        // ========================================
        public string WelcomeMessage => $"Welcome to Logistics Manager Dashboard, {Username}!";
        
        // For the sidebar profile
        public string UsernameInitial => Username?.Length > 0 ? Username[0].ToString().ToUpper() : "L";

        // Summary counts for dashboard cards
        public int TotalRequests => ProductRequests?.Count ?? 0;
        public int ApprovedRequests => ProductRequests?.Count(r => r.RequestStatus == "Approved") ?? 0;
        public int ReadyForShipmentRequests => ProductRequests?.Count(r => r.RequestStatus == "Ready for Shipment") ?? 0;
        
        // Shipment summary counts (for Shipments section)
        public int ApprovedShipmentCount => ShipmentRequests?.Count(r => r.RequestStatus == "Approved") ?? 0;
        public int ReadyForShipmentCount => ShipmentRequests?.Count(r => r.RequestStatus == "Ready for Shipment") ?? 0;
        public int SoldOutShipmentCount => ShipmentRequests?.Count(r => r.RequestStatus == "Sold Out") ?? 0;
        
        
        [ObservableProperty] private BaseRevenueViewModel? _revenueViewModel;
        
        [ObservableProperty] private ObservableCollection<ProductRequestWithUIId> _productRequestsWithIds;

        [ObservableProperty]
        private ObservableCollection<ProductRequestWithUIId> _shipmentRequestsWithIds;

        // Only these new shipment-specific properties
        [ObservableProperty] private int _currentShipmentPage = 1;
        [ObservableProperty] private int _totalShipmentPages = 1;
        [ObservableProperty] private ObservableCollection<ProductRequestWithUIId> _paginatedShipmentRequests;
        
        
        [ObservableProperty] private int _abortedShipmentCount;
        
        // a wrapper class for UI display
        public class ProductRequestWithUIId
        {
            public int UIId { get; set; }
            public ProductRequest Request { get; set; }

            // Properties to match the XAML bindings
            public string Product => Request?.Product?.Name ?? "N/A";
            public int RequestedQuantity => Request?.RequestedQuantity ?? 0;
            public string RequestedBy => Request?.RequestedBy ?? "N/A";
            public string RequestStatus => Request?.RequestStatus ?? "N/A";
            public DateTime RequestDate => Request?.RequestDate ?? DateTime.Now;
            public decimal TotalCost => Request?.TotalCost ?? 0;
    
            // Additional properties for XAML bindings
            public Product ProductObject => Request?.Product;  // For {Binding Product.Name}
            public bool IsApproved => RequestStatus == "Approved";
            public bool IsReadyForShipment => RequestStatus == "Ready for Shipment";
            public bool IsNotActionable => !IsApproved && !IsReadyForShipment;
        }
        
        // ========================================
        // CONSTRUCTOR
        // ========================================
        public LogisticsManagerDashboardViewModel(Action navigateToLogin, string username)
            : base(navigateToLogin, username, "Logistics Manager Dashboard")
        {
            RevenueViewModel = new BaseRevenueViewModel("Logistics Manager");
            // Initialize collections
            ShipmentRequestsWithIds = new ObservableCollection<ProductRequestWithUIId>();
            ProductRequests = new ObservableCollection<ProductRequest>();
            ShipmentRequests = new ObservableCollection<ProductRequest>();
            ProductRequestsWithIds = new ObservableCollection<ProductRequestWithUIId>();
            PaginatedRequests = new ObservableCollection<ProductRequestWithUIId>();
            
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
                    Console.WriteLine($"Logistics dashboard loading failed: {ex.Message}");
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

            CurrentSectionView = section switch
            {
                "Dashboard" => null, // Shows main dashboard content
                "Shipments" => new ShipmentsSection { DataContext = this },
                "InventoryL" => new InventoryLSection { DataContext = this },
                "Reports" => null, // Shows main dashboard for now
                _ => null
            };
    
            Console.WriteLine($"Logistics Manager navigated to {section}");
        }

        // ========================================
        // DATA LOADING
        // ========================================
        private async Task LoadDashboardDataAsync()
        {
            Console.WriteLine("Loading Logistics Manager dashboard data from API...");
            IsLoading = true;

            try
            {
                await ErrorHandler.TrySafeAsync("LoadLogisticsDashboard", async () =>
                {
                    Console.WriteLine("Loading product requests from API...");

                    var allRequests = await ProductRequestService.GetAllRequestsAsync();

                    // For Inventory Overview: Show Approved items that can be marked ready
                    var relevantRequests = allRequests.Where(r => 
                        r.RequestStatus == "Approved" || 
                        r.RequestStatus == "Ready for Shipment").ToList();

                    Console.WriteLine($"Loaded {relevantRequests.Count} relevant requests (Approved + Ready for Shipment) from API");
                    Console.WriteLine($"Loaded {relevantRequests.Count} relevant requests from API");

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        // Clear collections
                        ProductRequests.Clear();
                        ProductRequestsWithIds.Clear();

                        // Order by newest first, then add UI IDs
                        var orderedRequests = relevantRequests.OrderByDescending(r => r.RequestDate).ToList();

                        for (int i = 0; i < orderedRequests.Count; i++)
                        {
                            ProductRequests.Add(orderedRequests[i]);
                            ProductRequestsWithIds.Add(new ProductRequestWithUIId
                            {
                                UIId = i + 1,
                                Request = orderedRequests[i]
                            });
                        }

                        // Update pagination after loading data
                        UpdatePagination();

                        // Notify UI of count changes
                        OnPropertyChanged(nameof(TotalRequests));
                        OnPropertyChanged(nameof(ApprovedRequests));
                        OnPropertyChanged(nameof(ReadyForShipmentRequests));

                        Console.WriteLine(
                            $"Logistics dashboard updated - {ProductRequests.Count} requests displayed");
                        
                        var inventoryItems = ProductRequests.Count(r => r.RequestStatus == "Approved");
                        var shippedItems = allRequests.Count(r => r.RequestStatus == "Sold Out"); // All sold out orders
    
                        RevenueViewModel?.UpdateOrderProgress(
                            inventoryItems + shippedItems,  // Total items that were in inventory 
                            shippedItems,                   // Items successfully shipped (Sold Out)
                            inventoryItems                  // Items still in inventory (Approved)
                        );
                        Console.WriteLine($"Logistics Progress: {shippedItems} shipped out of {inventoryItems + shippedItems} inventory items");

                    });
                });

                await LoadShipmentDataAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadDashboardDataAsync error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                Console.WriteLine("Logistics dashboard data loading completed");
            }
        }

        private async Task LoadShipmentDataAsync()
        {
            Console.WriteLine("Loading shipment data...");

            try
            {
                var allRequests = await ProductRequestService.GetAllRequestsAsync();
        
                // Get both sold out AND aborted items
                var soldOutRequests = allRequests.Where(r => r.RequestStatus == "Sold Out").ToList();
                var abortedRequests = allRequests.Where(r => r.Notes?.Contains("ABORTED:") == true).ToList();
        
                // Combine both lists
                var shipmentRequests = soldOutRequests.Concat(abortedRequests).ToList();

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShipmentRequests.Clear();
                    ShipmentRequestsWithIds.Clear(); 
                    AbortedShipmentCount = abortedRequests.Count;

                    var orderedRequests = shipmentRequests.OrderByDescending(r => r.RequestDate).ToList();
    
                    for (int i = 0; i < orderedRequests.Count; i++)
                    {
                        ShipmentRequests.Add(orderedRequests[i]);
                        ShipmentRequestsWithIds.Add(new ProductRequestWithUIId
                        {
                            UIId = i + 1,
                            Request = orderedRequests[i]
                        });
                    }
    
                    // Notify UI of count changes
                    OnPropertyChanged(nameof(SoldOutShipmentCount));
                    OnPropertyChanged(nameof(AbortedShipmentCount));
                    UpdatePagination(isShipments: true);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shipment data: {ex.Message}");
            }
        }

        // ========================================
        // MARK READY FOR SHIPMENT COMMAND
        // ========================================
        [RelayCommand]
        private async Task MarkReadyForShipmentAsync(ProductRequest request)
        {
            if (request == null || request.RequestStatus != "Approved")
            {
                Console.WriteLine("Cannot mark ready for shipment: Invalid or non-approved request");
                return;
            }

            Console.WriteLine($"Marking request {request.Id} as Ready for Shipment - {request.Product?.Name}");

            try
            {
                // Update status from "Approved" to "Ready for Shipment"
                var success = await ProductRequestService.MarkReadyForShipmentAsync(request.Id, Username);
                
                if (success)
                {
                    Console.WriteLine($"Request {request.Id} status updated to 'Ready for Shipment'");
                    
                    // Reload data to reflect changes
                    await LoadDashboardDataAsync();
                }
                else
                {
                    Console.WriteLine($"Failed to update request {request.Id} status");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking ready for shipment: {ex.Message}");
            }
        }

        // ========================================
        // CANCEL READY FOR SHIPMENT COMMAND
        // ========================================
        [RelayCommand]
        private async Task CancelReadyForShipmentAsync(ProductRequest request)
        {
            if (request == null || request.RequestStatus != "Ready for Shipment")
            {
                Console.WriteLine("Cannot cancel: Request is not marked as Ready for Shipment");
                return;
            }

            Console.WriteLine($"Cancelling Ready for Shipment status for request {request.Id} - {request.Product?.Name}");

            try
            {
                // Update status from "Ready for Shipment" back to "Approved"
                var success = await ProductRequestService.UpdateRequestStatusAsync(request.Id, "Approved", Username, "Cancelled ready for shipment status");
                
                if (success)
                {
                    Console.WriteLine($"Request {request.Id} status reverted to 'Approved'");
                    
                    // Reload data to reflect changes
                    await LoadDashboardDataAsync();
                }
                else
                {
                    Console.WriteLine($"Failed to cancel ready for shipment status for request {request.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling ready for shipment: {ex.Message}");
            }
        }

        // ========================================
        // UTILITY COMMANDS
        // ========================================
        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            Console.WriteLine("Logistics Manager refreshing all data...");
            await LoadDashboardDataAsync();
        }
        
        // Pagination properties
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private int _itemsPerPage = 10;
        [ObservableProperty] private int _totalPages = 1;
        [ObservableProperty] private ObservableCollection<ProductRequestWithUIId> _paginatedRequests;

        // Pagination methods
        private void UpdatePagination(bool isShipments = false)
{
    if (isShipments)
    {
        // Shipment pagination logic
        if (ShipmentRequestsWithIds?.Count > 0)
        {
            TotalShipmentPages = (int)Math.Ceiling((double)ShipmentRequestsWithIds.Count / ItemsPerPage);
            CurrentShipmentPage = Math.Min(CurrentShipmentPage, TotalShipmentPages);
    
            var startIndex = (CurrentShipmentPage - 1) * ItemsPerPage;
            var pageItems = ShipmentRequestsWithIds.Skip(startIndex).Take(ItemsPerPage).ToList();
    
            if (PaginatedShipmentRequests == null)
                PaginatedShipmentRequests = new ObservableCollection<ProductRequestWithUIId>();
    
            PaginatedShipmentRequests.Clear();
            foreach (var item in pageItems) PaginatedShipmentRequests.Add(item);
        }
        else
        {
            // Handle empty shipment state
            TotalShipmentPages = 1;
            CurrentShipmentPage = 1;
            if (PaginatedShipmentRequests == null)
                PaginatedShipmentRequests = new ObservableCollection<ProductRequestWithUIId>();
            PaginatedShipmentRequests.Clear();
        }
    }
    else
    {
        // Proper pagination for inventory requests
        if (ProductRequestsWithIds?.Count > 0)
        {
            TotalPages = (int)Math.Ceiling((double)ProductRequestsWithIds.Count / ItemsPerPage);
            CurrentPage = Math.Min(CurrentPage, TotalPages);

            var startIndex = (CurrentPage - 1) * ItemsPerPage;
            var pageItems = ProductRequestsWithIds.Skip(startIndex).Take(ItemsPerPage).ToList();

            if (PaginatedRequests == null)
                PaginatedRequests = new ObservableCollection<ProductRequestWithUIId>();

            PaginatedRequests.Clear();
            foreach (var item in pageItems) PaginatedRequests.Add(item);
            
            Console.WriteLine($"Pagination updated: Page {CurrentPage}/{TotalPages}, showing {pageItems.Count} items");
        }
        else
        {
            // Handle empty state
            TotalPages = 1;
            CurrentPage = 1;
            if (PaginatedRequests == null)
                PaginatedRequests = new ObservableCollection<ProductRequestWithUIId>();
            PaginatedRequests.Clear();
        }

        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(PaginationText));
    }
}

        // Pagination navigation
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;
        public string PaginationText => $"Page {CurrentPage} of {TotalPages} ({ProductRequestsWithIds?.Count ?? 0} total items)";

        [RelayCommand]
        private void GoToPreviousPage()
        {
            if (CanGoPrevious)
            {
                CurrentPage--;
                UpdatePagination();
                
            }
            
        }

        [RelayCommand]
        private void GoToNextPage()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                UpdatePagination();
            }
        }
        
        [RelayCommand]
        private void GoToShipmentPreviousPage()
        {
            if (CurrentShipmentPage > 1)
            {
                CurrentShipmentPage--;
                UpdatePagination(isShipments: true);
            }
        }

        [RelayCommand]
        private void GoToShipmentNextPage()
        {
            if (CurrentShipmentPage < TotalShipmentPages)
            {
                CurrentShipmentPage++;
                UpdatePagination(isShipments: true);
            }
        }
        
    }
}