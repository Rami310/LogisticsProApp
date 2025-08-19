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
    public static class InventoryService
    {
        // Mock data for fallback (when API is unavailable)
        private static readonly List<InventoryItem> MockInventory = new()
        {
            new InventoryItem
            {
                Id = 1, ProductId = 1, Location = "A1-01", QuantityInStock = 250, MinimumStockLevel = 50,
                MaximumStockLevel = 500
            },
            new InventoryItem
            {
                Id = 2, ProductId = 2, Location = "A2-05", QuantityInStock = 30, MinimumStockLevel = 10,
                MaximumStockLevel = 50
            },
            new InventoryItem
            {
                Id = 3, ProductId = 3, Location = "B3-12", QuantityInStock = 5, MinimumStockLevel = 2,
                MaximumStockLevel = 10
            },
            new InventoryItem
            {
                Id = 4, ProductId = 4, Location = "C1-08", QuantityInStock = 120, MinimumStockLevel = 40,
                MaximumStockLevel = 200
            },
            new InventoryItem
            {
                Id = 5, ProductId = 5, Location = "B5-20", QuantityInStock = 8, MinimumStockLevel = 3,
                MaximumStockLevel = 15
            }
        };

        /// <summary>
        /// Get all inventory items from API with intelligent fallback
        /// </summary>
        public static async Task<List<InventoryItem>> GetAllInventoryAsync()
        {
            Console.WriteLine("📊 Loading inventory...");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var response = await ApiConfiguration.HttpClient.GetAsync("Inventory");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var inventoryDtos = JsonSerializer.Deserialize<List<InventoryItemDto>>(responseJson, ApiConfiguration.JsonOptions);
                        
                        if (inventoryDtos != null && inventoryDtos.Any())
                        {
                            var inventoryItems = inventoryDtos.Select(dto => new InventoryItem
                            {
                                Id = dto.Id,
                                ProductId = dto.ProductId,
                                WarehouseId = dto.WarehouseId,
                                Location = dto.Location,
                                QuantityInStock = dto.QuantityInStock,
                                MinimumStockLevel = dto.MinimumStockLevel,
                                MaximumStockLevel = dto.MaximumStockLevel,
                                LastStockUpdate = dto.LastStockUpdate,
                                Product = dto.Product != null ? new Product
                                {
                                    Id = dto.Product.Id,
                                    Name = dto.Product.Name,
                                    Description = dto.Product.Description,
                                    SKU = dto.Product.Sku,
                                    Category = dto.Product.Category,
                                    UnitPrice = dto.Product.UnitPrice,
                                    UnitOfMeasure = dto.Product.UnitOfMeasure,
                                    Supplier = dto.Product.Supplier,
                                    Status = dto.Product.Status
                                } : null
                            }).ToList();
                            
                            Console.WriteLine($"✅ Loaded {inventoryItems.Count} inventory items from API");
                            return inventoryItems;
                        }
                        else
                        {
                            Console.WriteLine("⚠️ API returned empty inventory list");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ API get inventory failed: {response.StatusCode}");
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error details: {errorContent}");
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ API not available, using mock inventory");
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏱️ API get inventory timeout, using mock data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API get inventory error: {ex.Message}");
            }

            // Fall back to mock data with product details
            Console.WriteLine("🔄 Using mock inventory data");
            return GetAllInventoryMock();
        }

        /// <summary>
        /// Get low stock items from API with fallback
        /// </summary>
        public static async Task<List<InventoryItem>> GetLowStockItemsAsync()
        {
            Console.WriteLine("📊 Loading low stock items...");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var response = await ApiConfiguration.HttpClient.GetAsync("Inventory/low-stock");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var inventoryDtos = JsonSerializer.Deserialize<List<InventoryItemDto>>(responseJson, ApiConfiguration.JsonOptions);
                        
                        if (inventoryDtos != null)
                        {
                            var inventoryItems = inventoryDtos.Select(dto => new InventoryItem
                            {
                                Id = dto.Id,
                                ProductId = dto.ProductId,
                                Location = dto.Location,
                                QuantityInStock = dto.QuantityInStock,
                                MinimumStockLevel = dto.MinimumStockLevel,
                                MaximumStockLevel = dto.MaximumStockLevel,
                                LastStockUpdate = dto.LastStockUpdate,
                                Product = dto.Product != null ? new Product
                                {
                                    Id = dto.Product.Id,
                                    Name = dto.Product.Name,
                                    SKU = dto.Product.Sku,
                                    Category = dto.Product.Category,
                                    Supplier = dto.Product.Supplier
                                } : null
                            }).ToList();
                            
                            Console.WriteLine($"✅ Loaded {inventoryItems.Count} low stock items from API");
                            return inventoryItems;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ API get low stock failed: {response.StatusCode}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏱️ API get low stock timeout, using mock data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API get low stock error: {ex.Message}");
            }

            // Fall back to mock data
            Console.WriteLine("🔄 Using mock low stock data");
            return GetLowStockItemsMock();
        }

        /// <summary>
        /// Get inventory item by ID from API with fallback
        /// </summary>
        public static async Task<InventoryItem?> GetInventoryItemByIdAsync(int id)
        {
            Console.WriteLine($"📊 Loading inventory item ID: {id}");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var response = await ApiConfiguration.HttpClient.GetAsync($"Inventory/{id}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var inventoryDto = JsonSerializer.Deserialize<InventoryItemDto>(responseJson, ApiConfiguration.JsonOptions);
                        
                        if (inventoryDto != null)
                        {
                            var inventoryItem = new InventoryItem
                            {
                                Id = inventoryDto.Id,
                                ProductId = inventoryDto.ProductId,
                                WarehouseId = inventoryDto.WarehouseId,
                                Location = inventoryDto.Location,
                                QuantityInStock = inventoryDto.QuantityInStock,
                                MinimumStockLevel = inventoryDto.MinimumStockLevel,
                                MaximumStockLevel = inventoryDto.MaximumStockLevel,
                                LastStockUpdate = inventoryDto.LastStockUpdate,
                                Product = inventoryDto.Product != null ? new Product
                                {
                                    Id = inventoryDto.Product.Id,
                                    Name = inventoryDto.Product.Name,
                                    SKU = inventoryDto.Product.Sku,
                                    Category = inventoryDto.Product.Category,
                                    UnitPrice = inventoryDto.Product.UnitPrice,
                                    Supplier = inventoryDto.Product.Supplier
                                } : null
                            };
                            
                            Console.WriteLine($"✅ Loaded inventory item from API: {inventoryItem.Product?.Name}");
                            return inventoryItem;
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine($"❌ Inventory item {id} not found in API");
                        return null; // Don't fall back for not found
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ API get inventory item failed: {response.StatusCode}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏱️ API get inventory item timeout, using mock data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API get inventory item error: {ex.Message}");
            }

            // Fall back to mock data
            Console.WriteLine("🔄 Using mock inventory data");
            var item = GetAllInventoryMock().FirstOrDefault(i => i.Id == id);
            return item;
        }

        /// <summary>
        /// Update stock levels via API with fallback
        /// </summary>
        public static async Task<bool> UpdateStockAsync(int productId, int quantity, bool isAddition = true)
        {
            Console.WriteLine($"📊 Updating stock for product {productId}: {(isAddition ? "+" : "-")}{quantity}");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var updateRequest = new
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        IsAddition = isAddition
                    };

                    var json = JsonSerializer.Serialize(updateRequest, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await ApiConfiguration.HttpClient.PutAsync("Inventory/stock", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"✅ Stock updated via API: {responseJson}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ API update stock failed: {response.StatusCode}");
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error details: {errorContent}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏱️ API update stock timeout, using mock update");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API update stock error: {ex.Message}");
            }

            // Fall back to mock update
            Console.WriteLine("🔄 Updating stock in mock data");
            return UpdateStock(productId, quantity, isAddition);
        }

        /// <summary>
        /// Add inventory item via API with fallback
        /// </summary>
        public static async Task<bool> AddInventoryItemAsync(InventoryItem item)
        {
            if (item == null)
            {
                Console.WriteLine("❌ Cannot add null inventory item");
                return false;
            }

            Console.WriteLine($"➕ Adding inventory item for product {item.ProductId}");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var createRequest = new
                    {
                        ProductId = item.ProductId,
                        WarehouseId = item.WarehouseId,
                        Location = item.Location,
                        QuantityInStock = item.QuantityInStock,
                        MinimumStockLevel = item.MinimumStockLevel,
                        MaximumStockLevel = item.MaximumStockLevel
                    };

                    var json = JsonSerializer.Serialize(createRequest, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await ApiConfiguration.HttpClient.PostAsync("Inventory", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ Inventory item added successfully via API");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ API add inventory failed: {response.StatusCode}");
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error details: {errorContent}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏱️ API add inventory timeout, using mock addition");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API add inventory error: {ex.Message}");
            }

            // Fall back to mock addition
            Console.WriteLine("🔄 Adding inventory item to mock data");
            AddInventoryItem(item);
            return true;
        }

        // ========================================
        // SYNCHRONOUS WRAPPER METHODS (Backward Compatibility)
        // ========================================

        /// <summary>
        /// Synchronous wrapper for GetAllInventoryAsync
        /// </summary>
        public static List<InventoryItem> GetAllInventory()
        {
            try
            {
                var task = GetAllInventoryAsync();
                task.Wait(ApiConfiguration.GetTimeout(true));
                return task.Result;
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                Console.WriteLine("⏱️ Sync get inventory timeout, using mock data");
                return GetAllInventoryMock();
            }
            catch
            {
                Console.WriteLine("❌ Sync get inventory failed, using mock data");
                return GetAllInventoryMock();
            }
        }

        /// <summary>
        /// Synchronous wrapper for GetLowStockItemsAsync
        /// </summary>
        public static List<InventoryItem> GetLowStockItems()
        {
            try
            {
                var task = GetLowStockItemsAsync();
                task.Wait(ApiConfiguration.GetTimeout(true));
                return task.Result;
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                Console.WriteLine("⏱️ Sync get low stock timeout, using mock data");
                return GetLowStockItemsMock();
            }
            catch
            {
                Console.WriteLine("❌ Sync get low stock failed, using mock data");
                return GetLowStockItemsMock();
            }
        }

        // ========================================
        // MOCK DATA METHODS (Fallback)
        // ========================================

        /// <summary>
        /// Get all inventory with joined product details (mock fallback)
        /// </summary>
        private static List<InventoryItem> GetAllInventoryMock()
        {
            // Join with products to get full details
            var products = ProductService.GetAllProducts();

            foreach (var item in MockInventory)
            {
                item.Product = products.FirstOrDefault(p => p.Id == item.ProductId);
            }

            return MockInventory.ToList();
        }

        /// <summary>
        /// Get low stock items from mock data
        /// </summary>
        private static List<InventoryItem> GetLowStockItemsMock()
        {
            return GetAllInventoryMock().Where(i => i.QuantityInStock <= i.MinimumStockLevel).ToList();
        }

        /// <summary>
        /// Get inventory item by ID from mock data
        /// </summary>
        public static InventoryItem? GetInventoryItemById(int id)
        {
            var item = GetAllInventory().FirstOrDefault(i => i.Id == id);
            return item;
        }

        /// <summary>
        /// Get inventory item by product ID from mock data
        /// </summary>
        public static InventoryItem? GetInventoryItemByProductId(int productId)
        {
            var item = GetAllInventory().FirstOrDefault(i => i.ProductId == productId);
            return item;
        }

        /// <summary>
        /// Add inventory item to mock data (fallback)
        /// </summary>
        public static void AddInventoryItem(InventoryItem item)
        {
            if (item == null) return;

            // Generate a new ID
            var newId = MockInventory.Count > 0 ? MockInventory.Max(i => i.Id) + 1 : 1;
            item.Id = newId;
            item.LastStockUpdate = DateTime.Now;

            MockInventory.Add(item);
            Console.WriteLine($"✅ Inventory item added to mock data with ID {newId}");
        }

        /// <summary>
        /// Update inventory item in mock data (fallback)
        /// </summary>
        public static void UpdateInventoryItem(InventoryItem item)
        {
            if (item == null) return;

            var existingItem = MockInventory.FirstOrDefault(i => i.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.ProductId = item.ProductId;
                existingItem.WarehouseId = item.WarehouseId;
                existingItem.Location = item.Location;
                existingItem.QuantityInStock = item.QuantityInStock;
                existingItem.MinimumStockLevel = item.MinimumStockLevel;
                existingItem.MaximumStockLevel = item.MaximumStockLevel;
                existingItem.LastStockUpdate = DateTime.Now;
                
                Console.WriteLine($"✅ Inventory item {item.Id} updated in mock data");
            }
            else
            {
                Console.WriteLine($"❌ Inventory item {item.Id} not found in mock data for update");
            }
        }

        /// <summary>
        /// Update stock levels in mock data (fallback)
        /// </summary>
        public static bool UpdateStock(int productId, int quantity, bool isAddition = true)
        {
            var item = MockInventory.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                Console.WriteLine($"❌ No inventory found for product {productId}");
                return false;
            }

            if (isAddition)
            {
                item.QuantityInStock += quantity;
                Console.WriteLine($"✅ Added {quantity} units to product {productId}. New stock: {item.QuantityInStock}");
            }
            else
            {
                if (item.QuantityInStock < quantity)
                {
                    Console.WriteLine($"❌ Insufficient stock for product {productId}. Available: {item.QuantityInStock}, Requested: {quantity}");
                    return false;
                }
                item.QuantityInStock -= quantity;
                Console.WriteLine($"✅ Removed {quantity} units from product {productId}. New stock: {item.QuantityInStock}");
            }

            item.LastStockUpdate = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Delete inventory item from mock data (fallback)
        /// </summary>
        public static bool DeleteInventoryItem(int id)
        {
            var item = MockInventory.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                var removed = MockInventory.Remove(item);
                if (removed)
                {
                    Console.WriteLine($"✅ Inventory item {id} deleted from mock data");
                }
                return removed;
            }
            
            Console.WriteLine($"❌ Inventory item {id} not found in mock data for deletion");
            return false;
        }
    }

    // ========================================
    // DTOs FOR API RESPONSES (Match API structure)
    // ========================================

    /// <summary>
    /// DTO for inventory items returned by API
    /// </summary>
    public class InventoryItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public string Location { get; set; } = string.Empty;
        public int QuantityInStock { get; set; }
        public int MinimumStockLevel { get; set; }
        public int MaximumStockLevel { get; set; }
        public DateTime LastStockUpdate { get; set; }
        public ProductDto? Product { get; set; }
    }

    /// <summary>
    /// DTO for products embedded in inventory responses
    /// </summary>
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string? Category { get; set; }
        public decimal UnitPrice { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? Supplier { get; set; }
        public string? Status { get; set; }
    }
}