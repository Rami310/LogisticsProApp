using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.Models;
using LogisticsPro.UI.Services;
using LogisticsPro.UI.Views.Warehouse.WarehouseEmployee.Sections;

namespace LogisticsPro.UI.ViewModels;

public partial class WarehouseEmployeeDashboardViewModel : BaseDashboardViewModel
{
    // ==============================================
    // SIMPLIFIED PROPERTIES - Only Approval Workflow
    // ==============================================
    [ObservableProperty] private ObservableCollection<ProductRequest> _pendingApprovals;

    [ObservableProperty] private ObservableCollection<ProductRequest> _approvedHistory;

    [ObservableProperty] private ObservableCollection<ProductRequest> _rejectedHistory;

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private ProductRequest _selectedRequest;

    // ========================================
    // DIALOG PROPERTIES - Only Approval Dialog
    // ========================================
    [ObservableProperty] private bool _isApprovalDialogOpen;

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================
    [ObservableProperty] private string _currentSection = "Dashboard";

    [ObservableProperty] private Control? _currentSectionView;



    public ICommand NavigateToReceivingCommand { get; }

    // ========================================
    // RECEIVING SECTION PROPERTIES (Real Data)
    // ========================================
    [ObservableProperty] private int _todayApprovedCount;
    public int PendingRequestsCount => PendingApprovals?.Count ?? 0;

    [ObservableProperty] private ObservableCollection<ProductRequest> _recentApprovedOrders;
    [ObservableProperty] private ObservableCollection<ProductRequest> _recentRejectedOrders;
    [ObservableProperty] private bool _showRejectionValidation;
    
    private string _approvalNotes = "";
    
    // ========================================
    // CONSTRUCTOR
    // ========================================
    public WarehouseEmployeeDashboardViewModel(Action navigateToLogin, string username)
        : base(navigateToLogin, username, "Warehouse Employee Dashboard")
    {
        // Initialize collections
        PendingApprovals = new ObservableCollection<ProductRequest>();
        ApprovedHistory = new ObservableCollection<ProductRequest>();
        RejectedHistory = new ObservableCollection<ProductRequest>();
        RecentApprovedOrders = new ObservableCollection<ProductRequest>();  
        RecentRejectedOrders = new ObservableCollection<ProductRequest>();

        NavigateToReceivingCommand = new RelayCommand(() => NavigateToSection("Receiving"));

        // Load data immediately
        _ = LoadDashboardDataAsync();
    }

    // ========================================
    // SIMPLIFIED DATA LOADING
    // ========================================
    private async Task LoadDashboardDataAsync()
    {
        IsLoading = true;

        try
        {
            Console.WriteLine("Loading employee dashboard data...");

            await ErrorHandler.TrySafeAsync("LoadEmployeeDashboardData", async () =>
            {
                // Get PENDING requests that need employee approval
                var pending = await ProductRequestService.GetRequestsByStatusAsync("Pending");
                Console.WriteLine($"Loaded {pending.Count} pending approval requests");

                // Get APPROVED requests (orders I approved)
                var approved = await ProductRequestService.GetRequestsByStatusAsync("Approved");
                var myApproved = approved.Where(r => r.ApprovedBy == Username).ToList();
                Console.WriteLine($"Loaded {myApproved.Count} requests I approved");

                // Get REJECTED requests (orders I rejected)
                var rejected = await ProductRequestService.GetRequestsByStatusAsync("Rejected");
                var myRejected = rejected.Where(r => r.ApprovedBy == Username).ToList();
                Console.WriteLine($"Loaded {myRejected.Count} requests I rejected");

                // Update collections on UI thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _allPendingApprovals.Clear();
                    foreach (var item in pending) _allPendingApprovals.Add(item);
    
                    PendingApprovals.Clear();
                    foreach (var item in pending) PendingApprovals.Add(item);
                    ApprovedHistory.Clear();
                    foreach (var item in myApproved) ApprovedHistory.Add(item);

                    RejectedHistory.Clear();
                    foreach (var item in myRejected) RejectedHistory.Add(item);
                    
                    RecentApprovedOrders.Clear();
                    var recentApproved = myApproved.OrderByDescending(r => r.ApprovalDate).Take(4).ToList();
                    foreach (var order in recentApproved) RecentApprovedOrders.Add(order);

                    // Load last 4 rejected orders for recent activity  
                    RecentRejectedOrders.Clear();
                    var recentRejected = myRejected.OrderByDescending(r => r.ApprovalDate).Take(4).ToList();
                    foreach (var order in recentRejected) RecentRejectedOrders.Add(order);

                    Console.WriteLine($"Recent activity - Approved: {RecentApprovedOrders.Count}, Rejected: {RecentRejectedOrders.Count}");

                    Console.WriteLine(
                        $"Employee dashboard loaded - Pending: {PendingApprovals.Count}, Approved: {ApprovedHistory.Count}, Rejected: {RejectedHistory.Count}");

                    // Calculate today's approved count
                    var today = DateTime.Today;
                    var todayApproved = myApproved.Count(r =>
                        r.ApprovalDate.HasValue && r.ApprovalDate.Value.Date == today);
                    TodayApprovedCount = todayApproved;

                    // Notify property changes for calculated properties
                    OnPropertyChanged(nameof(PendingRequestsCount));

                    Console.WriteLine(
                        $"Receiving metrics - Pending: {PendingRequestsCount}, Today Approved: {TodayApprovedCount}");
                });
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoadEmployeeDashboardData error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ========================================
    // APPROVAL WORKFLOW COMMANDS
    // ========================================
    [RelayCommand]
    private void OpenApprovalDialog(ProductRequest request)
    {
        if (request == null) return;
        SelectedRequest = request;
        ApprovalNotes = string.Empty;
        IsApprovalDialogOpen = true;
    }

    [RelayCommand]
    private void CloseApprovalDialog()
    {
        IsApprovalDialogOpen = false;
        SelectedRequest = null;
        ApprovalNotes = string.Empty;

    }

    [RelayCommand]
    private async Task ApproveRequestAsync()
    {
        if (SelectedRequest == null) return;

        await ErrorHandler.TrySafeAsync("ApproveRequest", async () =>
        {
            Console.WriteLine($"Approving request {SelectedRequest.Id} - This adds to inventory automatically");

            var notes = string.IsNullOrWhiteSpace(ApprovalNotes)
                ? "Approved - added to inventory"
                : ApprovalNotes;

            var success = await CallApprovalApiAsync(SelectedRequest.Id, "approve", notes);

            if (success)
            {
                Console.WriteLine($"Request {SelectedRequest.Id} approved - Inventory updated automatically");
                IsApprovalDialogOpen = false;
                await LoadDashboardDataAsync();
            }
        });
    }

    [RelayCommand]
    private async Task RejectRequestAsync()
    {
        if (SelectedRequest is null) return;

        // Show validation message if no reason provided
        if (string.IsNullOrWhiteSpace(ApprovalNotes))
        {
            ShowRejectionValidation = true;
            Console.WriteLine("Rejection reason is required");
            return;
        }

        // Hide validation message if reason is provided
        ShowRejectionValidation = false;

        await ErrorHandler.TrySafeAsync("RejectRequest", async () =>
        {
            Console.WriteLine($"Rejecting request {SelectedRequest.Id} - Money will be restored");

            var success = await CallApprovalApiAsync(SelectedRequest.Id, "reject", ApprovalNotes);

            if (success)
            {
                Console.WriteLine($"Request {SelectedRequest.Id} rejected - Revenue restored");
                IsApprovalDialogOpen = false;
                await LoadDashboardDataAsync();
            }
        });
    }

    public string ApprovalNotes
    {
        get => _approvalNotes;
        set
        {
            if (SetProperty(ref _approvalNotes, value))
            {
                // Hide validation message when user starts typing
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ShowRejectionValidation = false;
                }
            }
        }
    }
    
    private async Task<bool> CallApprovalApiAsync(int requestId, string action, string notes)
    {
        try
        {
            var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();

            if (apiAvailable)
            {
                var endpoint = $"ProductRequests/{requestId}/{action}";

                var requestBody = new
                {
                    ApprovedBy = Username,
                    RejectedBy = Username,
                    Notes = notes
                };

                var json = JsonSerializer.Serialize(requestBody, ApiConfiguration.JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ApiConfiguration.HttpClient.PutAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API call successful: {endpoint}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"API call failed: {endpoint} - {response.StatusCode}");
                }
            }
            else
            {
                Console.WriteLine("API not available, using mock approval");
                // Use both method names for compatibility
                ProductRequestService.UpdateRequestStatus(requestId, action == "approve" ? "Approved" : "Rejected",
                    Username);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call error for {action}: {ex.Message}");
        }

        return false;
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
            case "Receiving":
                Console.WriteLine("Creating ReceivingSection...");
                var receivingSection = new ReceivingSection();
                Console.WriteLine("Setting DataContext...");
                receivingSection.DataContext = this;
                Console.WriteLine("Setting CurrentSectionView...");
                CurrentSectionView = receivingSection;
                Console.WriteLine("Receiving section loaded");
                break;
            default:
                CurrentSectionView = null;
                break;
        }
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        SearchText = ""; // Clear search
        await LoadDashboardDataAsync();
    }

    private ObservableCollection<ProductRequest> _allPendingApprovals = new();
    private bool _isFiltering = false;

    private string _searchText = "";

    public string SearchText
    {
        get => _searchText;
        set
        {
            Console.WriteLine($"SearchText setter called with: '{value}'");

            if (SetProperty(ref _searchText, value) && !_isFiltering)
            {
                _isFiltering = true;
                try
                {
                    Console.WriteLine($"About to call FilterPendingRequests with: '{value}'");
                    FilterPendingRequests();
                    Console.WriteLine($"FilterPendingRequests completed");
                }
                finally
                {
                    _isFiltering = false;
                }
            }
            else
            {
                Console.WriteLine($"FilterPendingRequests NOT called - isFiltering: {_isFiltering}");
            }
        }
    }

// Move this OUTSIDE the property, as a separate method:
    [RelayCommand] 
    private void FilterPendingRequests()
    {
        Console.WriteLine($"Real-time search triggered with text: '{SearchText}'");

        ErrorHandler.TrySafe("FilterPendingRequests", () =>
        {
            // Convert both to List for consistency
            var sourceData = _allPendingApprovals.Any() ? _allPendingApprovals.ToList() : PendingApprovals.ToList();
            PendingApprovals.Clear();
            var searchLower = SearchText?.ToLower();

            var filtered = string.IsNullOrWhiteSpace(searchLower)
                ? sourceData
                : sourceData.Where(r =>
                    r.Product?.Name?.ToLower().Contains(searchLower) == true ||
                    r.Product?.SKU?.ToLower().Contains(searchLower) == true ||
                    r.RequestedBy?.ToLower().Contains(searchLower) == true);

            foreach (var item in filtered)
            {
                PendingApprovals.Add(item);
                // Add null check for the console output
                Console.WriteLine($"Added to filtered results: {item.Product?.Name ?? "Unknown"}");
            }

            Console.WriteLine($"Filtered {PendingApprovals.Count} items from {sourceData.Count} total");
        });
    }
}