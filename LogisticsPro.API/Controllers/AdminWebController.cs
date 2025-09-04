using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using LogisticsPro.API.Data;
using Microsoft.EntityFrameworkCore;

namespace LogisticsPro.API.Controllers
{
    [Route("admin")]
    public class AdminWebController : Controller
    {
        private readonly LogisticsDbContext _context;
        private readonly ILogger<AdminWebController> _logger;

        public AdminWebController(LogisticsDbContext context, ILogger<AdminWebController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Get data directly from database (more efficient than HTTP calls)
                var revenue = await _context.CompanyRevenue
                    .OrderByDescending(r => r.LastUpdated)
                    .FirstOrDefaultAsync();

                var totalStock = await _context.InventoryItems
                    .SumAsync(i => i.QuantityInStock);

                var pendingRequests = await _context.ProductRequests
                    .CountAsync(r => r.RequestStatus == "Pending");

                var model = new AdminDashboardModel
                {
                    AvailableBudget = revenue?.AvailableBudget ?? 0,
                    CurrentRevenue = revenue?.CurrentRevenue ?? 0,
                    TotalSpent = revenue?.TotalSpent ?? 0,
                    TotalStock = totalStock,
                    PendingRequests = pendingRequests,
                    LastUpdated = revenue?.LastUpdated ?? DateTime.Now
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return View(new AdminDashboardModel()); // Return empty model on error
            }
        }

        [HttpGet("charts-data")]
        public async Task<IActionResult> GetChartsData()
        {
            try
            {
                var transactions = await _context.RevenueTransactions
                    .OrderByDescending(t => t.CreatedDate)
                    .ToListAsync();

                var chartsData = new
                {
                    SpendingData = ProcessSpendingData(transactions),
                    IncomeData = ProcessIncomeData(transactions),
                    NetProfitData = ProcessNetProfitData(transactions)
                };

                return Json(chartsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting charts data");
                return Json(new { error = "Failed to load chart data" });
            }
        }

        private object ProcessSpendingData(List<LogisticsPro.API.Models.RevenueTransaction> transactions)
        {
            var chartData = new List<double>();
            var labels = new List<string>();

            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                var monthName = targetDate.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
                
                var monthlySpending = transactions
                    .Where(t => t.TransactionType == "ORDER_PLACED" && 
                               t.Amount > 0 &&
                               t.CreatedDate.Month == targetDate.Month && 
                               t.CreatedDate.Year == targetDate.Year)
                    .Sum(t => (double)t.Amount);

                chartData.Add(monthlySpending);
                labels.Add(monthName);
            }

            return new { labels, values = chartData };
        }

        private object ProcessIncomeData(List<LogisticsPro.API.Models.RevenueTransaction> transactions)
        {
            var chartData = new List<double>();
            var labels = new List<string>();

            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                var monthName = targetDate.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
                
                var monthlyIncome = transactions
                    .Where(t => t.TransactionType == "DELIVERY_CONFIRMED" && 
                               t.Amount > 0 &&
                               t.CreatedDate.Month == targetDate.Month && 
                               t.CreatedDate.Year == targetDate.Year)
                    .Sum(t => (double)t.Amount);

                chartData.Add(monthlyIncome);
                labels.Add(monthName);
            }

            return new { labels, values = chartData };
        }

        private object ProcessNetProfitData(List<LogisticsPro.API.Models.RevenueTransaction> transactions)
        {
            var chartData = new List<double>();
            var labels = new List<string>();

            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                var monthName = targetDate.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
                
                var monthlyIncome = transactions
                    .Where(t => t.TransactionType == "DELIVERY_CONFIRMED" && 
                               t.Amount > 0 &&
                               t.CreatedDate.Month == targetDate.Month && 
                               t.CreatedDate.Year == targetDate.Year)
                    .Sum(t => (double)t.Amount);

                var monthlySpending = transactions
                    .Where(t => t.TransactionType == "ORDER_PLACED" && 
                               t.Amount > 0 &&
                               t.CreatedDate.Month == targetDate.Month && 
                               t.CreatedDate.Year == targetDate.Year)
                    .Sum(t => (double)t.Amount);

                var netProfit = monthlyIncome - monthlySpending;
                chartData.Add(netProfit);
                labels.Add(monthName);
            }

            return new { labels, values = chartData };
        }
    }

    // Model class for the dashboard
    public class AdminDashboardModel
    {
        public decimal AvailableBudget { get; set; }
        public decimal CurrentRevenue { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalStock { get; set; }
        public int PendingRequests { get; set; }
        public DateTime LastUpdated { get; set; }

        public string AvailableBudgetDisplay => FormatCurrency(AvailableBudget);
        public string CurrentRevenueDisplay => FormatCurrency(CurrentRevenue);
        public string TotalSpentDisplay => FormatCurrency(TotalSpent);

        private string FormatCurrency(decimal amount)
        {
            return amount >= 1000000 
                ? $"${amount / 1000000:F1}M"
                : amount >= 1000
                ? $"${amount / 1000:F0}K" 
                : $"${amount:F0}";
        }
    }
}