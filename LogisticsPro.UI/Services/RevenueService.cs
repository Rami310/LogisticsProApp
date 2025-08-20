using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LogisticsPro.UI.Models;
using LogisticsPro.UI.Models.Revenue;

namespace LogisticsPro.UI.Services
{
    public static class RevenueService
    {
        // Mock data for fallback
        private static readonly CompanyRevenueDto MockRevenue = new()
        {
            CurrentRevenue = 1500000.00m,
            AvailableBudget = 1485000.00m,
            TotalSpent = 15000.00m,
            LastUpdated = DateTime.Now
        };

        /// <summary>
        /// Get current company revenue and available budget
        /// </summary>
        public static async Task<CompanyRevenueDto> GetCurrentRevenueAsync()  // ✅ FIXED: Return type
        {
            Console.WriteLine("💰 Loading company revenue...");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var response = await ApiConfiguration.HttpClient.GetAsync("Revenue/current");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var revenue = JsonSerializer.Deserialize<CompanyRevenueDto>(responseJson, ApiConfiguration.JsonOptions);
                        
                        if (revenue != null)
                        {
                            Console.WriteLine($"✅ Revenue loaded from API - Available: ${revenue.AvailableBudget:N0}");
                            return revenue;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ API get revenue failed: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API get revenue error: {ex.Message}");
            }

            // Fall back to mock data
            Console.WriteLine("🔄 Using mock revenue data");
            return MockRevenue;
        }

        /// <summary>
        /// Get monthly spending data
        /// </summary>
        public static async Task<MonthlySpendingDto> GetMonthlySpendingAsync()  // ✅ FIXED: Return type
        {
            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var response = await ApiConfiguration.HttpClient.GetAsync("Revenue/monthly-spending");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var monthlyData = JsonSerializer.Deserialize<MonthlySpendingDto>(responseJson, ApiConfiguration.JsonOptions);
                        
                        if (monthlyData != null)
                        {
                            return monthlyData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to get monthly spending: {ex.Message}");
            }

            // Fallback
            return new MonthlySpendingDto 
            { 
                CurrentMonthSpent = 5000.00m,
                PreviousMonthSpent = 4500.00m,
                Month = DateTime.Now.ToString("MMMM"),
                Year = DateTime.Now.Year
            };
        }

        /// <summary>
        /// Deduct money when manager places an order
        /// </summary>
        public static async Task<bool> DeductForOrderAsync(int productRequestId, decimal amount, string managerUsername)  // ✅ FIXED: Return type
        {
            Console.WriteLine($"💰 Deducting ${amount:F2} for order by {managerUsername}");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var request = new
                    {
                        ProductRequestId = productRequestId,
                        Amount = amount,
                        TransactionType = "ORDER_PLACED",
                        CreatedBy = managerUsername,
                        Description = $"Product order placed by {managerUsername}"
                    };

                    var json = JsonSerializer.Serialize(request, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await ApiConfiguration.HttpClient.PostAsync("Revenue/deduct", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ Revenue deducted: ${amount:F2}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ API deduct revenue failed: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to deduct revenue: {ex.Message}");
            }

            // For mock/fallback, just log the transaction
            Console.WriteLine($"🔄 Mock deduction: ${amount:F2} from revenue");
            return true; // Assume success for mock
        }

        /// <summary>
        /// Restore money when order is cancelled or rejected
        /// </summary>
        public static async Task<bool> RestoreForCancelledOrderAsync(int productRequestId, decimal amount, string username)  // ✅ FIXED: Return type
        {
            Console.WriteLine($"💰 Restoring ${amount:F2} for cancelled order by {username}");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var request = new
                    {
                        ProductRequestId = productRequestId,
                        Amount = amount,
                        TransactionType = "ORDER_CANCELLED",
                        CreatedBy = username,
                        Description = $"Product order cancelled by {username}"
                    };

                    var json = JsonSerializer.Serialize(request, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await ApiConfiguration.HttpClient.PostAsync("Revenue/restore", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ Revenue restored: ${amount:F2}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to restore revenue: {ex.Message}");
            }

            // Mock success
            Console.WriteLine($"🔄 Mock restoration: ${amount:F2} to revenue");
            return true;
        }

        /// <summary>
        /// Restore money when employee rejects an order
        /// </summary>
        public static async Task<bool> RestoreForRejectedOrderAsync(int productRequestId, decimal amount, string employeeUsername)  // ✅ FIXED: Return type
        {
            Console.WriteLine($"💰 Restoring ${amount:F2} for rejected order by {employeeUsername}");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var request = new
                    {
                        ProductRequestId = productRequestId,
                        Amount = amount,
                        TransactionType = "ORDER_REJECTED",
                        CreatedBy = employeeUsername,
                        Description = $"Product order rejected by {employeeUsername}"
                    };

                    var json = JsonSerializer.Serialize(request, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await ApiConfiguration.HttpClient.PostAsync("revenue/restore", content);                    
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ Revenue restored: ${amount:F2}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to restore revenue: {ex.Message}");
            }

            // Mock success
            Console.WriteLine($"🔄 Mock restoration: ${amount:F2} to revenue");
            return true;
        }
        
        /// <summary>
        /// Add profit when employee confirms delivery (Sold Out)
        /// </summary>
        public static async Task<bool> AddProfitForDeliveryAsync(int productRequestId, decimal totalCost, string employeeUsername)
        {
            // Calculate profit as total cost * 1.5
            var profitAmount = totalCost * 1.5m;
            Console.WriteLine($"💰 Adding profit ${profitAmount:F2} for delivered order (${totalCost:F2} * 1.5) by {employeeUsername}");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    var request = new
                    {
                        ProductRequestId = productRequestId,
                        Amount = profitAmount,
                        TransactionType = "DELIVERY_CONFIRMED",
                        CreatedBy = employeeUsername,
                        Description = $"Delivery confirmed and profit added by {employeeUsername}"
                    };

                    var json = JsonSerializer.Serialize(request, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Use the existing 'restore' endpoint to add profit to revenue
                    var response = await ApiConfiguration.HttpClient.PostAsync("Revenue/restore", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ Profit added: ${profitAmount:F2}");
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"⚠️ API add profit failed: {response.StatusCode}");
                        Console.WriteLine($"🔍 Error details: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to add profit: {ex.Message}");
            }

            // Mock success - in real implementation, you might update your MockRevenue
            Console.WriteLine($"🔄 Mock profit addition: ${profitAmount:F2} to revenue");
            return true;
        }
        

        /// <summary>
        /// Get basic revenue statistics for reports (Step 2)
        /// </summary>
        public static async Task<RevenueStatisticsDto> GetRevenueStatisticsAsync()
        {
            Console.WriteLine("📊 Loading revenue statistics for reports...");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();
                
                if (apiAvailable)
                {
                    // Get current revenue from your existing API
                    var currentRevenue = await GetCurrentRevenueAsync();
                    
                    // Get transactions from your existing API  
                    var response = await ApiConfiguration.HttpClient.GetAsync("Revenue/transactions");
                    
                    if (response.IsSuccessStatusCode && currentRevenue != null)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var transactions = JsonSerializer.Deserialize<List<RevenueTransactionDto>>(responseJson, ApiConfiguration.JsonOptions);
                        
                        if (transactions != null)
                        {
                            // Simple calculation: count DELIVERY_CONFIRMED transactions
                            var profitTransactions = transactions.Where(t => 
                                t.TransactionType == "DELIVERY_CONFIRMED" && t.Amount > 0).ToList();
                            
                            var currentMonth = DateTime.Now.Month;
                            var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                            
                            var thisMonthTransactions = profitTransactions.Where(t => 
                                t.CreatedDate.Month == currentMonth).ToList();
                            
                            var lastMonthTransactions = profitTransactions.Where(t => 
                                t.CreatedDate.Month == lastMonth).ToList();
                            
                            var statistics = new RevenueStatisticsDto
                            {
                                TotalProfit = profitTransactions.Sum(t => t.Amount),
                                CurrentMonthProfit = thisMonthTransactions.Sum(t => t.Amount),
                                LastMonthProfit = lastMonthTransactions.Sum(t => t.Amount),
                                CurrentMonthTransactions = thisMonthTransactions.Count,
                                LastMonthTransactions = lastMonthTransactions.Count,
                                LastUpdated = DateTime.Now
                            };
                            
                            Console.WriteLine($"✅ Statistics calculated - This month profit: ${statistics.CurrentMonthProfit:F2}");
                            return statistics;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting statistics: {ex.Message}");
            }

            // Simple fallback data
            Console.WriteLine("🔄 Using fallback statistics data");
            return new RevenueStatisticsDto
            {
                TotalProfit = 25000.00m,
                CurrentMonthProfit = 12450.00m,
                LastMonthProfit = 10500.00m,
                CurrentMonthTransactions = 47,
                LastMonthTransactions = 42,
                LastUpdated = DateTime.Now
            };
}
    }
}