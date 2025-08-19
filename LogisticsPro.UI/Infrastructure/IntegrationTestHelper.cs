using System;
using System.Linq;
using System.Threading.Tasks;
using LogisticsPro.UI.Services;

namespace LogisticsPro.UI.Infrastructure
{
    /// <summary>
    /// Helper class to test API integration during development
    /// </summary>
    public static class IntegrationTestHelper
    {
        /// <summary>
        /// Run comprehensive API integration tests
        /// </summary>
        public static async Task RunIntegrationTestsAsync()
        {
            Console.WriteLine("🧪 Starting API Integration Tests...");
            Console.WriteLine("=" + new string('=', 50));

            // Test 1: API Availability
            await TestApiAvailability();

            // Test 2: User Authentication
            await TestUserAuthentication();

            // Test 3: Product Loading
            await TestProductLoading();

            // Test 4: Inventory Loading
            await TestInventoryLoading();

            Console.WriteLine("=" + new string('=', 50));
            Console.WriteLine("🏁 Integration Tests Complete!");
        }

        private static async Task TestApiAvailability()
        {
            Console.WriteLine("\n🔍 Test 1: API Availability");
            Console.WriteLine("-" + new string('-', 30));

            try
            {
                var isAvailable = await ApiConfiguration.IsApiAvailableAsync();
                if (isAvailable)
                {
                    Console.WriteLine("✅ API is available and responding");
                }
                else
                {
                    Console.WriteLine("❌ API is not available");
                    Console.WriteLine("💡 Make sure your API project is running on https://localhost:7001");
                    Console.WriteLine("💡 Check: dotnet run in LogisticsPro.API folder");
                    Console.WriteLine("💡 Check: MYSQL_PASSWORD environment variable is set");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API availability check failed: {ex.Message}");
            }
        }

        private static async Task TestUserAuthentication()
        {
            Console.WriteLine("\n🔐 Test 2: User Authentication");
            Console.WriteLine("-" + new string('-', 30));

            var testAccounts = new[]
            {
                new { Username = "admin", Password = "1234", Role = "Administrator" },
                new { Username = "manager", Password = "5678", Role = "Warehouse Manager" },
                new { Username = "hrmanager", Password = "hr123", Role = "HR Manager" },
                new { Username = "employee", Password = "abcd", Role = "Employee" }
            };

            foreach (var account in testAccounts)
            {
                try
                {
                    var user = await UserService.ValidateUserAsync(account.Username, account.Password);
                    if (user != null)
                    {
                        Console.WriteLine($"✅ {account.Username} → {user.Role} ({user.Name} {user.LastName})");
                    }
                    else
                    {
                        Console.WriteLine($"❌ {account.Username} → Login failed");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ {account.Username} → Error: {ex.Message}");
                }
            }

            // Test invalid credentials
            try
            {
                var invalidUser = await UserService.ValidateUserAsync("invalid", "wrong");
                if (invalidUser == null)
                {
                    Console.WriteLine("✅ Invalid credentials correctly rejected");
                }
                else
                {
                    Console.WriteLine("❌ Invalid credentials incorrectly accepted");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Invalid credential test error: {ex.Message}");
            }
        }

        private static async Task TestProductLoading()
        {
            Console.WriteLine("\n📦 Test 3: Product Loading");
            Console.WriteLine("-" + new string('-', 30));

            try
            {
                var products = await ProductService.GetAllProductsAsync();
                if (products != null && products.Count > 0)
                {
                    Console.WriteLine($"✅ Loaded {products.Count} products from API");
                    foreach (var product in products.Take(3)) // Show first 3
                    {
                        Console.WriteLine($"   • {product.Name} ({product.SKU}) - ${product.UnitPrice}");
                    }
                    if (products.Count > 3)
                    {
                        Console.WriteLine($"   ... and {products.Count - 3} more products");
                    }
                }
                else
                {
                    Console.WriteLine("❌ No products loaded");
                }

                // Test specific product lookup
                var specificProduct = await ProductService.GetProductBySKUAsync("BOX-STD-001");
                if (specificProduct != null)
                {
                    Console.WriteLine($"✅ Product lookup by SKU successful: {specificProduct.Name}");
                }
                else
                {
                    Console.WriteLine("❌ Product lookup by SKU failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Product loading failed: {ex.Message}");
            }
        }

        private static async Task TestInventoryLoading()
        {
            Console.WriteLine("\n📊 Test 4: Inventory Loading");
            Console.WriteLine("-" + new string('-', 30));

            try
            {
                var inventory = await InventoryService.GetAllInventoryAsync();
                if (inventory != null && inventory.Count > 0)
                {
                    Console.WriteLine($"✅ Loaded {inventory.Count} inventory items from API");
                    foreach (var item in inventory.Take(3)) // Show first 3
                    {
                        var productName = item.Product?.Name ?? "Unknown Product";
                        Console.WriteLine($"   • {productName} @ {item.Location}: {item.QuantityInStock} units");
                    }
                    if (inventory.Count > 3)
                    {
                        Console.WriteLine($"   ... and {inventory.Count - 3} more items");
                    }
                }
                else
                {
                    Console.WriteLine("❌ No inventory items loaded");
                }

                // Test low stock items
                var lowStock = await InventoryService.GetLowStockItemsAsync();
                if (lowStock != null)
                {
                    Console.WriteLine($"✅ Found {lowStock.Count} low stock items");
                    foreach (var item in lowStock.Take(2)) // Show first 2
                    {
                        var productName = item.Product?.Name ?? "Unknown Product";
                        Console.WriteLine($"   • {productName}: {item.QuantityInStock}/{item.MinimumStockLevel} (LOW)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Inventory loading failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Quick test to verify basic connectivity
        /// </summary>
        public static async Task<bool> QuickConnectivityTestAsync()
        {
            try
            {
                Console.WriteLine("🔌 Running quick connectivity test...");
                
                var isAvailable = await ApiConfiguration.IsApiAvailableAsync();
                if (isAvailable)
                {
                    Console.WriteLine("✅ API connectivity confirmed");
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ API not reachable - will use mock data");
                    return false;
                }
            }
            catch
            {
                Console.WriteLine("❌ Connectivity test failed - will use mock data");
                return false;
            }
        }

        /// <summary>
        /// Print helpful debugging information
        /// </summary>
        public static void PrintDebugInfo()
        {
            Console.WriteLine("\n🔧 Debug Information");
            Console.WriteLine("-" + new string('-', 30));
            Console.WriteLine($"API Base URL: {ApiConfiguration.BaseUrl}");
            Console.WriteLine($"Default Timeout: {ApiConfiguration.DefaultTimeout.TotalSeconds}s");
            Console.WriteLine($"Quick Timeout: {ApiConfiguration.QuickTimeout.TotalSeconds}s");
            Console.WriteLine($"Current Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            Console.WriteLine("\n💡 Troubleshooting Tips:");
            Console.WriteLine("• Make sure LogisticsPro.API is running");
            Console.WriteLine("• Check that the API is accessible at https://localhost:7001");
            Console.WriteLine("• Verify your MySQL connection string");
            Console.WriteLine("• Check firewall settings");
            Console.WriteLine("• Look for CORS errors in browser dev tools");
        }

        /// <summary>
        /// Test specific API endpoint manually
        /// </summary>
        public static async Task TestSpecificEndpointAsync(string endpoint)
        {
            Console.WriteLine($"\n🎯 Testing specific endpoint: {endpoint}");
            Console.WriteLine("-" + new string('-', 30));

            try
            {
                var client = ApiConfiguration.HttpClient;
                var response = await client.GetAsync(endpoint);
                
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Content Type: {response.Content.Headers.ContentType}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✅ Success! Content length: {content.Length} characters");
                    
                    if (content.Length < 500) // Show short responses
                    {
                        Console.WriteLine($"Response: {content}");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Failed: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
            }
        }
    }
}