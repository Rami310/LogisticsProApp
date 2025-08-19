using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticsPro.API.Data;
using LogisticsPro.API.Models;
using LogisticsPro.API.Models.RequestModels;

namespace LogisticsPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RevenueController : ControllerBase
    {
        private readonly LogisticsDbContext _context;

        public RevenueController(LogisticsDbContext context)
        {
            _context = context;
            Console.WriteLine("RevenueController created successfully");
        }

        [HttpGet("current")]
        public async Task<ActionResult<object>> GetCurrentRevenue()
        {
            Console.WriteLine("GET /api/Revenue/current called");
            
            try
            {
                Console.WriteLine("Querying CompanyRevenue table...");
                
                var revenue = await _context.CompanyRevenue
                    .OrderByDescending(r => r.LastUpdated)
                    .FirstOrDefaultAsync();

                Console.WriteLine($"Query result: {(revenue != null ? "Found revenue record" : "No revenue record found")}");

                if (revenue == null)
                {
                    Console.WriteLine("No revenue record found - creating initial one");
                    
                    revenue = new CompanyRevenue
                    {
                        CurrentRevenue = 1500000.00m,
                        AvailableBudget = 1499795.25m,
                        TotalSpent = 204.75m,
                        UpdatedBy = "system",
                        UpdateReason = "Initial revenue setup - auto-created",
                        LastUpdated = DateTime.Now
                    };

                    _context.CompanyRevenue.Add(revenue);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine("Initial revenue record created successfully");
                }

                var result = new
                {
                    currentRevenue = revenue.CurrentRevenue,
                    availableBudget = revenue.AvailableBudget,
                    totalSpent = revenue.TotalSpent,
                    lastUpdated = revenue.LastUpdated
                };

                Console.WriteLine($"Returning revenue data - Available: ${revenue.AvailableBudget:F2}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetCurrentRevenue: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                
                return StatusCode(500, new { 
                    error = "Failed to retrieve revenue data", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("monthly-spending")]
        public async Task<ActionResult<object>> GetMonthlySpending()
        {
            Console.WriteLine("GET /api/Revenue/monthly-spending called");
            
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                var previousYear = currentMonth == 1 ? currentYear - 1 : currentYear;

                Console.WriteLine($"Calculating spending for {currentMonth}/{currentYear} vs {previousMonth}/{previousYear}");

                var currentMonthSpent = await _context.RevenueTransactions
                    .Where(t => t.CreatedDate.Month == currentMonth && 
                               t.CreatedDate.Year == currentYear &&
                               (t.TransactionType == "ORDER_PLACED"))
                    .SumAsync(t => t.Amount);

                var previousMonthSpent = await _context.RevenueTransactions
                    .Where(t => t.CreatedDate.Month == previousMonth && 
                               t.CreatedDate.Year == previousYear &&
                               (t.TransactionType == "ORDER_PLACED"))
                    .SumAsync(t => t.Amount);

                var result = new
                {
                    currentMonthSpent = currentMonthSpent,
                    previousMonthSpent = previousMonthSpent,
                    month = DateTime.Now.ToString("MMMM"),
                    year = currentYear
                };

                Console.WriteLine($"Monthly spending - Current: ${currentMonthSpent:F2}, Previous: ${previousMonthSpent:F2}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetMonthlySpending: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve monthly spending", details = ex.Message });
            }
        }

        [HttpPost("deduct")]
        public async Task<ActionResult<object>> DeductForOrder([FromBody] DeductRevenueRequest request)
        {
            Console.WriteLine($"POST /api/Revenue/deduct called - Amount: ${request.Amount:F2}");
            
            try
            {
                if (request.Amount <= 0)
                {
                    Console.WriteLine("Invalid amount - must be greater than zero");
                    return BadRequest(new { error = "Amount must be greater than zero" });
                }

                Console.WriteLine("Getting current revenue record...");

                var revenue = await _context.CompanyRevenue
                    .OrderByDescending(r => r.LastUpdated)
                    .FirstOrDefaultAsync();

                if (revenue == null)
                {
                    Console.WriteLine("No revenue record found");
                    return NotFound(new { error = "Company revenue record not found" });
                }

                Console.WriteLine($"Current budget: ${revenue.AvailableBudget:F2}, Requested: ${request.Amount:F2}");

                if (revenue.AvailableBudget < request.Amount)
                {
                    Console.WriteLine("Insufficient budget for request");
                    return BadRequest(new { 
                        error = "Insufficient budget", 
                        available = revenue.AvailableBudget,
                        requested = request.Amount 
                    });
                }

                Console.WriteLine("Sufficient budget available - processing deduction");

                if (request.ProductRequestId.HasValue)
                {
                    var productRequestExists = await _context.ProductRequests
                        .AnyAsync(pr => pr.Id == request.ProductRequestId.Value);
                        
                    if (!productRequestExists)
                    {
                        Console.WriteLine($"ProductRequest {request.ProductRequestId} not found - creating transaction without foreign key");
                        request.ProductRequestId = null;
                    }
                    else
                    {
                        Console.WriteLine($"ProductRequest {request.ProductRequestId} exists - safe to reference");
                    }
                }

                var transaction = new RevenueTransaction
                {
                    TransactionType = request.TransactionType ?? "ORDER_PLACED",
                    Amount = request.Amount,
                    ProductRequestId = request.ProductRequestId,
                    CreatedBy = request.CreatedBy ?? "unknown",
                    CreatedDate = DateTime.Now,
                    Description = request.Description ?? $"Order deduction by {request.CreatedBy}",
                    BalanceAfter = revenue.AvailableBudget - request.Amount
                };

                _context.RevenueTransactions.Add(transaction);

                revenue.AvailableBudget -= request.Amount;
                revenue.TotalSpent += request.Amount;
                revenue.UpdatedBy = request.CreatedBy ?? "unknown";
                revenue.UpdateReason = $"Order placed - Deducted ${request.Amount:F2}";
                revenue.LastUpdated = DateTime.Now;

                Console.WriteLine("Saving changes to database...");
                await _context.SaveChangesAsync();

                var result = new
                {
                    success = true,
                    message = $"Successfully deducted ${request.Amount:F2}",
                    newAvailableBudget = revenue.AvailableBudget,
                    totalSpent = revenue.TotalSpent,
                    transactionId = transaction.Id
                };

                Console.WriteLine($"Revenue deducted successfully - New Balance: ${revenue.AvailableBudget:F2}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in DeductForOrder: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Failed to deduct revenue", details = ex.Message });
            }
        }

        [HttpPost("restore")]
        public async Task<ActionResult<object>> RestoreForOrder([FromBody] RestoreRevenueRequest request)
        {
            Console.WriteLine($"POST /api/Revenue/restore called - Amount: ${request.Amount:F2}");
            
            try
            {
                if (request.Amount <= 0)
                {
                    Console.WriteLine("Invalid amount - must be greater than zero");
                    return BadRequest(new { error = "Amount must be greater than zero" });
                }

                var revenue = await _context.CompanyRevenue
                    .OrderByDescending(r => r.LastUpdated)
                    .FirstOrDefaultAsync();

                if (revenue == null)
                {
                    Console.WriteLine("No revenue record found");
                    return NotFound(new { error = "Company revenue record not found" });
                }

                Console.WriteLine($"Restoring ${request.Amount:F2} to budget");

                if (request.ProductRequestId.HasValue)
                {
                    var productRequestExists = await _context.ProductRequests
                        .AnyAsync(pr => pr.Id == request.ProductRequestId.Value);
                        
                    if (!productRequestExists)
                    {
                        Console.WriteLine($"ProductRequest {request.ProductRequestId} not found - creating transaction without foreign key");
                        request.ProductRequestId = null;
                    }
                }

                var transaction = new RevenueTransaction
                {
                    TransactionType = request.TransactionType ?? "ORDER_CANCELLED",
                    Amount = request.Amount,
                    ProductRequestId = request.ProductRequestId,
                    CreatedBy = request.CreatedBy ?? "unknown",
                    CreatedDate = DateTime.Now,
                    Description = request.Description ?? $"Order restoration by {request.CreatedBy}",
                    BalanceAfter = revenue.AvailableBudget + request.Amount
                };

                _context.RevenueTransactions.Add(transaction);

                revenue.AvailableBudget += request.Amount;
                revenue.TotalSpent = Math.Max(0, revenue.TotalSpent - request.Amount);
                revenue.UpdatedBy = request.CreatedBy ?? "unknown";
                revenue.UpdateReason = $"Order cancelled/rejected - Restored ${request.Amount:F2}";
                revenue.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();

                var result = new
                {
                    success = true,
                    message = $"Successfully restored ${request.Amount:F2}",
                    newAvailableBudget = revenue.AvailableBudget,
                    totalSpent = revenue.TotalSpent,
                    transactionId = transaction.Id
                };

                Console.WriteLine($"Revenue restored successfully - New Balance: ${revenue.AvailableBudget:F2}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in RestoreForOrder: {ex.Message}");
                return StatusCode(500, new { error = "Failed to restore revenue", details = ex.Message });
            }
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<IEnumerable<object>>> GetTransactionHistory([FromQuery] int? limit = 50)
        {
            Console.WriteLine($"GET /api/Revenue/transactions called - Limit: {limit}");
            
            try
            {
                var transactions = await _context.RevenueTransactions
                    .OrderByDescending(t => t.CreatedDate)
                    .Take(limit ?? 50)
                    .Select(t => new
                    {
                        id = t.Id,
                        transactionType = t.TransactionType,
                        amount = t.Amount,
                        productRequestId = t.ProductRequestId,
                        createdBy = t.CreatedBy,
                        createdDate = t.CreatedDate,
                        description = t.Description,
                        balanceAfter = t.BalanceAfter
                    })
                    .ToListAsync();

                Console.WriteLine($"Retrieved {transactions.Count} transactions");
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetTransactionHistory: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve transactions", details = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetRevenueStatistics()
        {
            Console.WriteLine("GET /api/Revenue/statistics called");
            
            try
            {
                var revenue = await _context.CompanyRevenue
                    .OrderByDescending(r => r.LastUpdated)
                    .FirstOrDefaultAsync();

                if (revenue == null)
                {
                    return NotFound(new { error = "Revenue data not found" });
                }

                var budgetUtilization = revenue.CurrentRevenue > 0 
                    ? (double)(revenue.TotalSpent / revenue.CurrentRevenue) * 100 
                    : 0;

                var totalTransactions = await _context.RevenueTransactions.CountAsync();
                
                var recentSpending = await _context.RevenueTransactions
                    .Where(t => t.CreatedDate >= DateTime.Now.AddDays(-30) && 
                               t.TransactionType == "ORDER_PLACED")
                    .SumAsync(t => t.Amount);

                var result = new
                {
                    currentRevenue = revenue.CurrentRevenue,
                    availableBudget = revenue.AvailableBudget,
                    totalSpent = revenue.TotalSpent,
                    budgetUtilizationPercentage = Math.Round(budgetUtilization, 2),
                    totalTransactions = totalTransactions,
                    last30DaysSpending = recentSpending,
                    lastUpdated = revenue.LastUpdated,
                    budgetStatus = budgetUtilization switch
                    {
                        < 50 => "Healthy",
                        < 75 => "Moderate",
                        < 90 => "High",
                        _ => "Critical"
                    }
                };

                Console.WriteLine($"Revenue statistics - Utilization: {budgetUtilization:F1}%");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetRevenueStatistics: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve statistics", details = ex.Message });
            }
        }

        [HttpPut("adjust")]
        public async Task<ActionResult<object>> AdjustRevenue([FromBody] AdjustRevenueRequest request)
        {
            Console.WriteLine("PUT /api/Revenue/adjust called");
            
            try
            {
                var revenue = await _context.CompanyRevenue
                    .OrderByDescending(r => r.LastUpdated)
                    .FirstOrDefaultAsync();

                if (revenue == null)
                {
                    return NotFound(new { error = "Company revenue record not found" });
                }

                var oldCurrentRevenue = revenue.CurrentRevenue;
                var oldAvailableBudget = revenue.AvailableBudget;

                if (request.NewCurrentRevenue.HasValue)
                {
                    revenue.CurrentRevenue = request.NewCurrentRevenue.Value;
                    var difference = request.NewCurrentRevenue.Value - oldCurrentRevenue;
                    revenue.AvailableBudget += difference;
                }

                if (request.BudgetAdjustment.HasValue)
                {
                    revenue.AvailableBudget += request.BudgetAdjustment.Value;
                }

                revenue.UpdatedBy = request.UpdatedBy ?? "admin";
                revenue.UpdateReason = request.Reason ?? "Manual revenue adjustment";
                revenue.LastUpdated = DateTime.Now;

                var transaction = new RevenueTransaction
                {
                    TransactionType = "REVENUE_ADJUSTMENT",
                    Amount = request.BudgetAdjustment ?? (request.NewCurrentRevenue - oldCurrentRevenue) ?? 0,
                    ProductRequestId = null,
                    CreatedBy = request.UpdatedBy ?? "admin",
                    CreatedDate = DateTime.Now,
                    Description = request.Reason ?? "Revenue adjustment",
                    BalanceAfter = revenue.AvailableBudget
                };

                _context.RevenueTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                var result = new
                {
                    success = true,
                    message = "Revenue adjusted successfully",
                    oldValues = new { currentRevenue = oldCurrentRevenue, availableBudget = oldAvailableBudget },
                    newValues = new { currentRevenue = revenue.CurrentRevenue, availableBudget = revenue.AvailableBudget },
                    transactionId = transaction.Id
                };

                Console.WriteLine($"Revenue adjusted - Old: ${oldCurrentRevenue:F2}, New: ${revenue.CurrentRevenue:F2}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in AdjustRevenue: {ex.Message}");
                return StatusCode(500, new { error = "Failed to adjust revenue", details = ex.Message });
            }
        }
    }
}