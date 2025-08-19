using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LogisticsPro.UI.Models;

namespace LogisticsPro.UI.Services
{
    public static class ProductRequestService
    {
        // Mock data for fallback (matches your database seeder)
        private static readonly List<ProductRequest> MockRequests = new()
        {
            // PENDING: Manager created, waiting for EMPLOYEE decision
            new ProductRequest
            {
                Id = 1,
                ProductId = 1,
                RequestedQuantity = 100,
                RequestedBy = "warehouse_mgr",
                RequestStatus = "Pending",
                RequestDate = DateTime.Now.AddHours(-2),
                Notes = "Regular restock - Low inventory alert",
                TotalCost = 2500.00m,
                CreatedBy = "warehouse_mgr"
            },

            new ProductRequest
            {
                Id = 2,
                ProductId = 3,
                RequestedQuantity = 5,
                RequestedBy = "warehouse_mgr",
                RequestStatus = "Pending",
                RequestDate = DateTime.Now.AddHours(-1),
                Notes = "Emergency replacement needed",
                TotalCost = 750.00m,
                CreatedBy = "warehouse_mgr"
            },

            // APPROVED: Employee approved = Received + In Inventory
            new ProductRequest
            {
                Id = 3,
                ProductId = 2,
                RequestedQuantity = 15,
                RequestedBy = "warehouse_mgr",
                RequestStatus = "Approved",
                ApprovedBy = "wh_emp1",
                ApprovalDate = DateTime.Now.AddDays(-1),
                RequestDate = DateTime.Now.AddDays(-2),
                Notes = "Monthly restock\n[APPROVED] Quality verified, added to inventory - wh_emp1",
                TotalCost = 450.00m,
                CreatedBy = "warehouse_mgr"
            },

            // REJECTED: Employee rejected with mandatory reason
            new ProductRequest
            {
                Id = 4,
                ProductId = 4,
                RequestedQuantity = 50,
                RequestedBy = "warehouse_mgr",
                RequestStatus = "Rejected",
                ApprovedBy = "wh_emp1", // Using ApprovedBy to track who made decision
                ApprovalDate = DateTime.Now.AddDays(-1),
                RequestDate = DateTime.Now.AddDays(-2),
                Notes = "Safety equipment order\n[REJECTED] Quality concerns - items damaged in shipping - wh_emp1",
                TotalCost = 875.00m,
                CreatedBy = "warehouse_mgr"
            },

            // CANCELLED: Manager cancelled before employee decision
            new ProductRequest
            {
                Id = 5,
                ProductId = 5,
                RequestedQuantity = 10,
                RequestedBy = "warehouse_mgr",
                RequestStatus = "Cancelled",
                RequestDate = DateTime.Now.AddDays(-3),
                Notes = "Special project supplies\n[CANCELLED] Project postponed - warehouse_mgr",
                TotalCost = 125.00m,
                CreatedBy = "warehouse_mgr"
            }
        };
    
        // ========================================
        // ASYNC API METHODS (Primary)
        // ========================================

        /// <summary>
        /// Get all product requests from API with fallback
        /// </summary>
        public static async Task<List<ProductRequest>> GetAllRequestsAsync()
        {
            Console.WriteLine("Loading all product requests...");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var response = await ApiConfiguration.HttpClient.GetAsync("ProductRequests");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var requests = JsonSerializer.Deserialize<List<ProductRequest>>(responseJson, ApiConfiguration.JsonOptions);
                        
                        if (requests != null && requests.Any())
                        {
                            // Join with products to get full product details if needed
                            var products = await ProductService.GetAllProductsAsync();
                            foreach (var request in requests)
                            {
                                if (request.Product == null)
                                {
                                    request.Product = products.FirstOrDefault(p => p.Id == request.ProductId) ?? new Product();
                                }
                            }
                            
                            Console.WriteLine($"Loaded {requests.Count} product requests from API");
                            return requests;
                        }
                        else
                        {
                            Console.WriteLine("API returned empty request list");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"API get requests failed: {response.StatusCode}");
                    }
                }
                else
                {
                    Console.WriteLine("API not available, using mock requests");
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("API get requests timeout, using mock data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API get requests error: {ex.Message}");
            }

            // Fall back to mock data with product details
            Console.WriteLine("Using mock request data");
            return GetAllRequestsMock();
        }

        /// <summary>
        /// Get requests by status from API with fallback
        /// </summary>
        public static async Task<List<ProductRequest>> GetRequestsByStatusAsync(string status)
        {
            Console.WriteLine($"Loading {status} product requests...");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var response = await ApiConfiguration.HttpClient.GetAsync($"ProductRequests/status/{status}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var requests = JsonSerializer.Deserialize<List<ProductRequest>>(responseJson, ApiConfiguration.JsonOptions);
                        
                        if (requests != null)
                        {
                            // Join with products to get full product details if needed
                            var products = await ProductService.GetAllProductsAsync();
                            foreach (var request in requests)
                            {
                                if (request.Product == null)
                                {
                                    request.Product = products.FirstOrDefault(p => p.Id == request.ProductId) ?? new Product();
                                }
                            }
                            
                            Console.WriteLine($"Loaded {requests.Count} {status} requests from API");
                            return requests;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"API get {status} requests failed: {response.StatusCode}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"API get {status} requests timeout, using mock data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API get {status} requests error: {ex.Message}");
            }

            // Fall back to mock data
            Console.WriteLine($"Using mock {status} request data");
            return GetAllRequestsMock().Where(r => r.RequestStatus == status).ToList();
        }

        /// <summary>
        /// Get requests by user (async version)
        /// </summary>
        public static async Task<List<ProductRequest>> GetRequestsByUserAsync(string username)
        {
            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
        
                if (apiAvailable)
                {
                    var response = await ApiConfiguration.HttpClient.GetAsync($"ProductRequests/user/{username}");
            
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var requests = JsonSerializer.Deserialize<List<ProductRequest>>(responseJson, ApiConfiguration.JsonOptions);
                
                        if (requests != null)
                        {
                            // Join with products to get full product details if needed
                            var products = await ProductService.GetAllProductsAsync();
                            foreach (var request in requests)
                            {
                                if (request.Product == null)
                                {
                                    request.Product = products.FirstOrDefault(p => p.Id == request.ProductId) ?? new Product();
                                }
                            }
                    
                            Console.WriteLine($"Loaded {requests.Count} requests for user {username} from API");
                            return requests;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"API get user requests failed: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API get user requests error: {ex.Message}");
            }

            // Fall back to mock data
            Console.WriteLine($"Using mock user requests for {username}");
            return GetRequestsByUser(username);
        }

        /// <summary>
        /// Add product request via API with fallback
        /// </summary>
        public static async Task<bool> AddRequestAsync(ProductRequest request)
        {
            if (request == null)
            {
                Console.WriteLine("Cannot add null product request");
                return false;
            }

            Console.WriteLine($"Adding product request for {request.RequestedQuantity}x product {request.ProductId}");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();

                if (apiAvailable)
                {
                    // Match the exact fields expected by API
                    var createRequest = new
                    {
                        ProductId = request.ProductId,
                        RequestedQuantity = request.RequestedQuantity,
                        RequestedBy = request.RequestedBy,
                        Notes = request.Notes ?? "" // Ensure Notes is never null
                    };

                    var json = JsonSerializer.Serialize(createRequest, ApiConfiguration.JsonOptions);

                    // Debug: Log the JSON being sent
                    Console.WriteLine($"Sending to API: {json}");

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await ApiConfiguration.HttpClient.PostAsync("ProductRequests", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Product request added successfully via API: {responseJson}");
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API add request failed: {response.StatusCode}");
                        Console.WriteLine($"Error details: {errorContent}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("API add request timeout, using mock addition");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API add request error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }

            // Fall back to mock addition
            Console.WriteLine("Adding request to mock data");
            AddRequest(request);
            return true;
        }

        /// <summary>
        /// Cancel request via API with fallback
        /// </summary>
        public static async Task<bool> CancelRequestAsync(int id, string username)
        {
            Console.WriteLine($"Cancelling request {id}...");
    
            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
        
                if (apiAvailable)
                {
                    // Use the format that Swagger expects
                    var cancelData = new
                    {
                        cancelledBy = username,
                        notes = "Cancelled by user request"
                    };
            
                    var json = JsonSerializer.Serialize(cancelData, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
            
                    Console.WriteLine($"Sending to API: {json}");
            
                    var response = await ApiConfiguration.HttpClient.PutAsync($"ProductRequests/{id}/cancel", content);
            
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Request {id} cancelled via API: {responseContent}");
                
                        // Also update mock data as backup
                        UpdateRequestStatusMock(id, "Cancelled", username);
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API cancel failed: {response.StatusCode}");
                        Console.WriteLine($"Error details: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API cancel error: {ex.Message}");
            }
    
            // Fall back to mock update only
            Console.WriteLine("Falling back to mock update only");
            UpdateRequestStatusMock(id, "Cancelled", username);
            return false;
        }

        // ========================================
        // SYNCHRONOUS WRAPPER METHODS
        // ========================================

        /// <summary>
        /// Synchronous wrapper for GetAllRequestsAsync
        /// </summary>
        public static List<ProductRequest> GetAllRequests()
        {
            try
            {
                // Use Task.Run with proper timeout and error handling
                var task = Task.Run(async () => await GetAllRequestsAsync());
                
                // Wait with timeout to prevent infinite hanging
                if (task.Wait(TimeSpan.FromSeconds(5)))
                {
                    return task.Result;
                }
                else
                {
                    Console.WriteLine("Sync get requests timed out, using mock data");
                    return GetAllRequestsMock();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync get requests failed: {ex.Message}, using mock data");
                return GetAllRequestsMock();
            }
        }

        /// <summary>
        /// Get requests by status (synchronous)
        /// </summary>
        public static List<ProductRequest> GetRequestsByStatus(string status)
        {
            try
            {
                // Use Task.Run with proper timeout and error handling
                var task = Task.Run(async () => await GetRequestsByStatusAsync(status));
                
                // Wait with timeout to prevent infinite hanging
                if (task.Wait(TimeSpan.FromSeconds(5)))
                {
                    return task.Result;
                }
                else
                {
                    Console.WriteLine($"Sync get {status} requests timed out, using mock data");
                    return GetAllRequestsMock().Where(r => r.RequestStatus == status).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync get {status} requests failed: {ex.Message}, using mock data");
                return GetAllRequestsMock().Where(r => r.RequestStatus == status).ToList();
            }
        }

        // ========================================
        // MOCK DATA METHODS (Fallback)
        // ========================================

        /// <summary>
        /// Get all requests with product details (mock fallback)
        /// </summary>
        private static List<ProductRequest> GetAllRequestsMock()
        {
            // Join with products to get full details
            var products = ProductService.GetAllProducts();

            foreach (var request in MockRequests)
            {
                request.Product = products.FirstOrDefault(p => p.Id == request.ProductId) ?? new Product();
            }

            return MockRequests.ToList();
        }

        /// <summary>
        /// Get requests by user (mock)
        /// </summary>
        public static List<ProductRequest> GetRequestsByUser(string username)
        {
            return GetAllRequests().Where(r => r.RequestedBy == username).ToList();
        }

        /// <summary>
        /// Get request by ID (mock)
        /// </summary>
        public static ProductRequest GetRequestById(int id)
        {
            var request = MockRequests.FirstOrDefault(r => r.Id == id);
            if (request != null)
            {
                request.Product = ProductService.GetProductById(request.ProductId) ?? new Product();
            }

            return request;
        }

        /// <summary>
        /// Add request to mock data (fallback)
        /// </summary>
        public static void AddRequest(ProductRequest request)
        {
            // Generate a new ID
            var newId = MockRequests.Count > 0 ? MockRequests.Max(r => r.Id) + 1 : 1;
            request.Id = newId;
            request.RequestDate = DateTime.Now;
            request.RequestStatus = "Pending";

            MockRequests.Add(request);
            Console.WriteLine($"Request {newId} added to mock data");
        }

        /// <summary>
        /// Update request status in mock data (fallback)
        /// </summary>
        public static void UpdateRequestStatus(int id, string status, string username)
        {
            var request = MockRequests.FirstOrDefault(r => r.Id == id);
            if (request != null)
            {
                request.RequestStatus = status;

                switch (status)
                {
                    case "Approved":
                        request.ApprovedBy = username;
                        request.ApprovalDate = DateTime.Now;
                        break;
                    case "Received":
                        request.ReceivedBy = username;
                        request.ReceivedDate = DateTime.Now;

                        // Update inventory
                        InventoryService.UpdateStock(request.ProductId, request.RequestedQuantity, true);
                        break;
                    case "Rejected":
                        // Record who rejected it
                        request.ApprovedBy = username; // Using ApprovedBy to record who took action
                        request.ApprovalDate = DateTime.Now;
                        break;
                }
                
                Console.WriteLine($"Request {id} status updated to {status} in mock data");
            }
            else
            {
                Console.WriteLine($"Request {id} not found in mock data for update");
            }
        }

        /// <summary>
        /// Update request status in mock data (fallback) - Alternative method name
        /// </summary>
        public static void UpdateRequestStatusMock(int id, string status, string username)
        {
            UpdateRequestStatus(id, status, username);
        }

        /// <summary>
        /// Delete request from mock data
        /// </summary>
        public static bool DeleteRequest(int id)
        {
            var request = MockRequests.FirstOrDefault(r => r.Id == id);
            if (request != null)
            {
                var removed = MockRequests.Remove(request);
                if (removed)
                {
                    Console.WriteLine($"Request {id} deleted from mock data");
                }
                return removed;
            }
            
            Console.WriteLine($"Request {id} not found in mock data for deletion");
            return false;
        }

        /// <summary>
        /// Get the count of pending requests
        /// </summary>
        public static int GetPendingRequestCount()
        {
            return GetRequestsByStatus("Pending").Count;
        }
        
        /// <summary>
        /// Update request status via API with fallback
        /// </summary>
        public static async Task<bool> UpdateRequestStatusAsync(int id, string status, string username, string notes = "")
        {
            Console.WriteLine($"Updating request {id} to {status}...");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();

                if (apiAvailable)
                {
                    var statusData = new
                    {
                        status = status,
                        updatedBy = username,
                        notes = notes
                    };

                    var json = JsonSerializer.Serialize(statusData, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    Console.WriteLine($"Sending to API: {json}");

                    var response = await ApiConfiguration.HttpClient.PutAsync($"ProductRequests/{id}/status", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Request {id} updated to {status} via API: {responseContent}");

                        // Also update mock data as backup
                        UpdateRequestStatusMock(id, status, username);
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API status update failed: {response.StatusCode}");
                        Console.WriteLine($"Error details: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API status update error: {ex.Message}");
            }

            // Fall back to mock update only
            Console.WriteLine("Falling back to mock update only");
            UpdateRequestStatusMock(id, status, username);
            return true; // Return true for mock success
        }
        
        /// <summary>
        /// Update request to Pending Order status via API with fallback
        /// </summary>
        public static async Task<bool> OrderShipmentAsync(int id, string username)
        {
            Console.WriteLine($"Updating request {id} to Pending Order...");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();

                if (apiAvailable)
                {
                    // Use the /status endpoint that exists in your API
                    var statusData = new
                    {
                        status = "Pending Order",
                        updatedBy = username,
                        notes = "Shipment ordered by logistics manager"
                    };

                    var json = JsonSerializer.Serialize(statusData, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    Console.WriteLine($"Sending to API: {json}");

                    // Use the existing /status endpoint
                    var response = await ApiConfiguration.HttpClient.PutAsync($"ProductRequests/{id}/status", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Request {id} updated to Pending Order via API: {responseContent}");

                        // Also update mock data as backup
                        UpdateRequestStatusMock(id, "Pending Order", username);
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API order shipment failed: {response.StatusCode}");
                        Console.WriteLine($"Error details: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API order shipment error: {ex.Message}");
            }

            // Fall back to mock update only
            Console.WriteLine("Falling back to mock update only");
            UpdateRequestStatusMock(id, "Pending Order", username);
            return true; // Return true for mock success
        }
        
        /// <summary>
        /// Mark request as Ready for Shipment via API with fallback
        /// </summary>
        public static async Task<bool> MarkReadyForShipmentAsync(int id, string username)
        {
            Console.WriteLine($"Marking request {id} as Ready for Shipment...");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();

                if (apiAvailable)
                {
                    // Use the existing /status endpoint
                    var statusData = new
                    {
                        status = "Ready for Shipment",
                        updatedBy = username,
                        notes = "Marked ready for shipment by logistics manager"
                    };

                    var json = JsonSerializer.Serialize(statusData, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    Console.WriteLine($"Sending to API: {json}");

                    // Use the existing /status endpoint
                    var response = await ApiConfiguration.HttpClient.PutAsync($"ProductRequests/{id}/status", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Request {id} marked as Ready for Shipment via API: {responseContent}");

                        // Also update mock data as backup
                        UpdateRequestStatusMock(id, "Ready for Shipment", username);
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API mark ready for shipment failed: {response.StatusCode}");
                        Console.WriteLine($"Error details: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API mark ready for shipment error: {ex.Message}");
            }

            // Fall back to mock update only
            Console.WriteLine("Falling back to mock update only");
            UpdateRequestStatusMock(id, "Ready for Shipment", username);
            return true; // Return true for mock success
        }
        
        /// <summary>
        /// Confirm delivery and update inventory
        /// </summary>
        public static async Task<bool> ConfirmDeliveryAsync(int id, string username)
        {
            Console.WriteLine($"Confirming delivery for request {id}...");

            try
            {
                // First update status to "Sold Out"
                var success = await UpdateRequestStatusAsync(id, "Sold Out", username, $"Delivery confirmed by {username} on {DateTime.Now:MM/dd HH:mm}");        
                if (success)
                {
                    // Get the request to access product and quantity info
                    var allRequests = await GetAllRequestsAsync();
                    var request = allRequests.FirstOrDefault(r => r.Id == id);
            
                    if (request != null)
                    {
                        // Remove from inventory (reduce stock)
                        await InventoryService.UpdateStockAsync(request.ProductId, -request.RequestedQuantity, false);
                        Console.WriteLine($"Removed {request.RequestedQuantity} units of product {request.ProductId} from inventory");
                
                        // Add profit (1.5x original cost = total cost * 1.5)
                        if (request.TotalCost > 0)
                        {
                            // Actually add the profit to revenue
                            try
                            {
                                await RevenueService.AddProfitForDeliveryAsync(request.Id, request.TotalCost, username);
                                Console.WriteLine($"Profit added to company revenue for successful delivery");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to add profit to revenue: {ex.Message}");
                                // calculate for logging
                                var profit = request.TotalCost * 0.5m;
                                Console.WriteLine($"Profit calculated: ${profit:F2} for successful delivery");
                            }
                        }
                    }
                }
        
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error confirming delivery: {ex.Message}");
                return false;
            }
        }
    }
}