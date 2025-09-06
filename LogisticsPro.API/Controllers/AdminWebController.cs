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
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");
            
            try
            {
                // Get the logged-in user's details
                var username = HttpContext.Session.GetString("AdminUsername");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

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
                
                ViewBag.AdminUser = user?.Name + " " + user?.LastName ?? username;
                ViewBag.AdminRole = user?.Role ?? "Admin";
        
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
            if (!IsAdminLoggedIn())
                return Unauthorized();
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
        
        [HttpGet("login")]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("AdminLoggedIn") == "true")
                return RedirectToAction("Dashboard");
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && 
                                          u.Status == "Active" && 
                                          (u.Role == "Admin" || u.Role == "admin" || u.Role == "Administrator"));

            if (user != null && user.Password == password)
            {
                HttpContext.Session.SetString("AdminLoggedIn", "true");
                HttpContext.Session.SetString("AdminUsername", user.Username);
                HttpContext.Session.SetString("AdminUserId", user.Id.ToString());

                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid credentials or insufficient permissions";
            return View();
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("AdminLoggedIn") == "true";
        }

    }

    
}