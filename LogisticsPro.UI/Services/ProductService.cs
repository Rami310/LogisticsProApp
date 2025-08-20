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
    public static class ProductService
    {
        // Mock data for fallback (when API is unavailable)
        private static readonly List<Product> MockProducts = new()
        {
            new Product
            {
                Id = 1, Name = "Standard Cardboard Box", SKU = "BOX-STD-001", Category = "Packaging", UnitPrice = 2.50M,
                UnitOfMeasure = "Each", Supplier = "PackWell Inc.",
                Description = "Standard size cardboard box for general shipping"
            },
            new Product
            {
                Id = 2, Name = "Bubble Wrap (Small)", SKU = "WRAP-BUB-S", Category = "Packaging", UnitPrice = 15.75M,
                UnitOfMeasure = "Roll", Supplier = "SafePack", Description = "Small bubble wrap roll, 50ft"
            },
            new Product
            {
                Id = 3, Name = "Pallet Jack", SKU = "EQUIP-PAL-001", Category = "Equipment", UnitPrice = 299.99M,
                UnitOfMeasure = "Each", Supplier = "Heavy Lifters Co.",
                Description = "Standard manual pallet jack, 5000 lb capacity"
            },
            new Product
            {
                Id = 4, Name = "Shipping Labels", SKU = "LAB-SHP-100", Category = "Stationery", UnitPrice = 9.99M,
                UnitOfMeasure = "Pack", Supplier = "OfficeMart", Description = "Pack of 100 adhesive shipping labels"
            },
            new Product
            {
                Id = 5, Name = "Forklift Battery", SKU = "EQUIP-FORK-BAT", Category = "Equipment", UnitPrice = 1200.00M,
                UnitOfMeasure = "Each", Supplier = "PowerLift",
                Description = "Replacement battery for standard warehouse forklift"
            }
        };

        /// <summary>
        /// Get all products from API with intelligent fallback
        /// </summary>
        public static async Task<List<Product>> GetAllProductsAsync()
        {
            Console.WriteLine("Loading products...");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();

                if (apiAvailable)
                {
                    var response = await ApiConfiguration.HttpClient.GetAsync("Products");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var products =
                            JsonSerializer.Deserialize<List<Product>>(responseJson, ApiConfiguration.JsonOptions);

                        if (products != null && products.Any())
                        {
                            Console.WriteLine($"Loaded {products.Count} products from API");
                            return products;
                        }
                        else
                        {
                            Console.WriteLine("API returned empty product list");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"API get products failed: {response.StatusCode}");
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error details: {errorContent}");
                    }
                }
                else
                {
                    Console.WriteLine("API not available, using mock products");
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("API get products timeout, using mock data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API get products error: {ex.Message}");
            }

            // Fall back to mock data
            Console.WriteLine("Using mock product data");
            return MockProducts.ToList();
        }

        /// <summary>
        /// Get product by ID from API with fallback
        /// </summary>
        public static async Task<Product?> GetProductByIdAsync(int id)
        {
            Console.WriteLine($"Loading product ID: {id}");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();

                if (apiAvailable)
                {
                    var response = await ApiConfiguration.HttpClient.GetAsync($"Products/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var product = JsonSerializer.Deserialize<Product>(responseJson, ApiConfiguration.JsonOptions);

                        if (product != null)
                        {
                            Console.WriteLine($"Loaded product from API: {product.Name}");
                            return product;
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine($"Product {id} not found in API");
                        return null; // Don't fall back for not found
                    }
                    else
                    {
                        Console.WriteLine($"API get product failed: {response.StatusCode}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("API get product timeout, using mock data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API get product error: {ex.Message}");
            }

            // Fall back to mock data
            Console.WriteLine("Using mock product data");
            return MockProducts.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Get product by SKU from API with fallback
        /// </summary>
        public static async Task<Product?> GetProductBySKUAsync(string sku)
        {
            Console.WriteLine($"Loading product SKU: {sku}");

            if (string.IsNullOrWhiteSpace(sku))
            {
                Console.WriteLine("SKU cannot be empty");
                return null;
            }

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();

                if (apiAvailable)
                {
                    var response = await ApiConfiguration.HttpClient.GetAsync($"Products/sku/{sku}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var product = JsonSerializer.Deserialize<Product>(responseJson, ApiConfiguration.JsonOptions);

                        if (product != null)
                        {
                            Console.WriteLine($"Loaded product from API by SKU: {product.Name}");
                            return product;
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine($"Product with SKU {sku} not found in API");
                        return null; // Don't fall back for not found
                    }
                    else
                    {
                        Console.WriteLine($"API get product by SKU failed: {response.StatusCode}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("API get product by SKU timeout, using mock data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API get product by SKU error: {ex.Message}");
            }

            // Fall back to mock data
            Console.WriteLine("Using mock product data");
            return MockProducts.FirstOrDefault(p => p.SKU?.Equals(sku, StringComparison.OrdinalIgnoreCase) == true);
        }

        /// <summary>
        /// Add product via API with fallback
        /// </summary>
        public static async Task<bool> AddProductAsync(Product product)
        {
            if (product == null)
            {
                Console.WriteLine("Cannot add null product");
                return false;
            }

            Console.WriteLine($"Adding product: {product.Name}");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();

                if (apiAvailable)
                {
                    var json = JsonSerializer.Serialize(product, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await ApiConfiguration.HttpClient.PostAsync("Products", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Product {product.Name} added successfully via API");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"API add product failed: {response.StatusCode}");
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error details: {errorContent}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("API add product timeout, using mock addition");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API add product error: {ex.Message}");
            }

            // Fall back to mock addition
            Console.WriteLine("Adding product to mock data");
            AddProduct(product);
            return true;
        }
        
        
        // ========================================
        // SYNCHRONOUS WRAPPER METHODS 
        // ========================================

        /// <summary>
        /// Synchronous wrapper for GetAllProductsAsync
        /// </summary>
        public static List<Product> GetAllProducts()
        {
            try
            {
                var task = GetAllProductsAsync();
                task.Wait(ApiConfiguration.GetTimeout(true));
                return task.Result;
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                Console.WriteLine("Sync get products timeout, using mock data");
                return MockProducts.ToList();
            }
            catch
            {
                Console.WriteLine("Sync get products failed, using mock data");
                return MockProducts.ToList();
            }
        }

        /// <summary>
        /// Synchronous wrapper for GetProductByIdAsync
        /// </summary>
        public static Product? GetProductById(int id)
        {
            try
            {
                var task = GetProductByIdAsync(id);
                task.Wait(ApiConfiguration.GetTimeout(true));
                return task.Result;
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                Console.WriteLine("Sync get product timeout, using mock data");
                return MockProducts.FirstOrDefault(p => p.Id == id);
            }
            catch
            {
                Console.WriteLine("Sync get product failed, using mock data");
                return MockProducts.FirstOrDefault(p => p.Id == id);
            }
        }

        /// <summary>
        /// Synchronous wrapper for GetProductBySKUAsync
        /// </summary>
        public static Product? GetProductBySKU(string sku)
        {
            try
            {
                var task = GetProductBySKUAsync(sku);
                task.Wait(ApiConfiguration.GetTimeout(true));
                return task.Result;
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                Console.WriteLine("Sync get product by SKU timeout, using mock data");
                return MockProducts.FirstOrDefault(p => p.SKU?.Equals(sku, StringComparison.OrdinalIgnoreCase) == true);
            }
            catch
            {
                Console.WriteLine("Sync get product by SKU failed, using mock data");
                return MockProducts.FirstOrDefault(p => p.SKU?.Equals(sku, StringComparison.OrdinalIgnoreCase) == true);
            }
        }

        // ========================================
        // MOCK DATA METHODS (Fallback)
        // ========================================

        /// <summary>
        /// Add product to mock data (fallback)
        /// </summary>
        public static void AddProduct(Product product)
        {
            if (product == null) return;

            // Generate a new ID for mock data
            var newId = MockProducts.Count > 0 ? MockProducts.Max(p => p.Id) + 1 : 1;
            product.Id = newId;
            product.CreatedDate = DateTime.Now;

            MockProducts.Add(product);
            Console.WriteLine($"Product {product.Name} added to mock data with ID {newId}");
        }

        /// <summary>
        /// Update product in mock data (fallback)
        /// </summary>
        public static void UpdateProduct(Product product)
        {
            if (product == null) return;

            var existingProduct = MockProducts.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.SKU = product.SKU;
                existingProduct.Category = product.Category;
                existingProduct.UnitPrice = product.UnitPrice;
                existingProduct.UnitOfMeasure = product.UnitOfMeasure;
                existingProduct.Supplier = product.Supplier;
                existingProduct.Status = product.Status;
                existingProduct.UpdatedDate = DateTime.Now;

                Console.WriteLine($"Product {product.Name} updated in mock data");
            }
            else
            {
                Console.WriteLine($"Product {product.Id} not found in mock data for update");
            }
        }

        /// <summary>
        /// Delete product from mock data (fallback)
        /// </summary>
        public static bool DeleteProduct(int id)
        {
            var product = MockProducts.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                var removed = MockProducts.Remove(product);
                if (removed)
                {
                    Console.WriteLine($"Product {product.Name} deleted from mock data");
                }

                return removed;
            }

            Console.WriteLine($"Product {id} not found in mock data for deletion");
            return false;
        }
    }
}