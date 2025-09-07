using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using LogisticsPro.API.Data;
using Microsoft.EntityFrameworkCore;

namespace LogisticsPro.API.Controllers
{
    /// <summary>
    /// Web-based admin controller providing a responsive dashboard interface for administrative users.
    /// Handles authentication, dashboard data visualization, and financial reporting through a web browser.
    /// Accessible from any device with internet connectivity for remote monitoring.
    /// </summary>
    [Route("admin")]
    public class AdminWebController : Controller
    {
        private readonly LogisticsDbContext _context;
        private readonly ILogger<AdminWebController> _logger;

        /// <summary>
        /// Initializes a new instance of the AdminWebController.
        /// </summary>
        /// <param name="context">The database context for accessing logistics data</param>
        /// <param name="logger">Logger instance for tracking controller operations and errors</param>
        public AdminWebController(LogisticsDbContext context, ILogger<AdminWebController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Displays the main admin dashboard with KPI cards and navigation to charts.
        /// Requires admin authentication. Shows real-time company metrics including budget,
        /// stock levels, and pending requests.
        /// </summary>
        /// <returns>The dashboard view with populated AdminDashboardModel, or redirect to login if not authenticated</returns>
        /// <response code="200">Returns the dashboard view with current metrics</response>
        /// <response code="302">Redirects to login if user is not authenticated</response>
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

        /// <summary>
        /// Provides JSON data for the three financial charts displayed on the dashboard.
        /// Returns spending trends, income trends, and net profit calculations over the last 6 months.
        /// Used by JavaScript on the dashboard to render interactive charts.
        /// </summary>
        /// <returns>JSON object containing chart data for spending, income, and net profit</returns>
        /// <response code="200">Returns chart data in JSON format</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="500">If there was an error processing chart data</response>
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

        /// <summary>
        /// Processes revenue transactions to calculate monthly spending trends.
        /// Filters for ORDER_PLACED transactions and aggregates by month over the last 6 months.
        /// </summary>
        /// <param name="transactions">List of all revenue transactions</param>
        /// <returns>Object containing month labels and corresponding spending values</returns>
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

        /// <summary>
        /// Processes revenue transactions to calculate monthly income trends.
        /// Filters for DELIVERY_CONFIRMED transactions and aggregates by month over the last 6 months.
        /// </summary>
        /// <param name="transactions">List of all revenue transactions</param>
        /// <returns>Object containing month labels and corresponding income values</returns>
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

        /// <summary>
        /// Calculates net profit by subtracting monthly spending from monthly income.
        /// Combines ORDER_PLACED (expenses) and DELIVERY_CONFIRMED (revenue) transactions
        /// to show true profitability trends over the last 6 months.
        /// </summary>
        /// <param name="transactions">List of all revenue transactions</param>
        /// <returns>Object containing month labels and corresponding net profit values (can be negative)</returns>
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
        
        /// <summary>
        /// Displays the admin login page.
        /// If user is already authenticated, redirects to dashboard.
        /// </summary>
        /// <returns>Login view or redirect to dashboard if already logged in</returns>
        [HttpGet("login")]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("AdminLoggedIn") == "true")
                return RedirectToAction("Dashboard");
            return View();
        }

        /// <summary>
        /// Processes admin login authentication.
        /// Validates credentials against users with admin privileges and establishes session.
        /// </summary>
        /// <param name="username">The username for authentication</param>
        /// <param name="password">The password for authentication</param>
        /// <returns>Redirect to dashboard on success, or login view with error message on failure</returns>
        /// <response code="302">Redirects to dashboard on successful login</response>
        /// <response code="200">Returns login view with error message on failed authentication</response>
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

        /// <summary>
        /// Logs out the current admin user by clearing the session and redirecting to login.
        /// </summary>
        /// <returns>Redirect to login page</returns>
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Checks if the current user has a valid admin session.
        /// Used internally to protect admin-only endpoints and views.
        /// </summary>
        /// <returns>True if user is authenticated as admin, false otherwise</returns>
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("AdminLoggedIn") == "true";
        }
    }
}