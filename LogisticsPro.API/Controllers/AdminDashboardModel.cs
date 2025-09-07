namespace LogisticsPro.API.Controllers
{
    /// <summary>
    /// Data model representing key performance indicators (KPIs) and metrics displayed on the admin web dashboard.
    /// Contains financial information, inventory statistics, and operational data used to provide executives
    /// with real-time visibility into company performance from any web-enabled device.
    /// </summary>
    public class AdminDashboardModel
    {
        /// <summary>
        /// The current available budget that can be spent on new orders and operations.
        /// Calculated as CurrentRevenue minus TotalSpent. Displayed prominently as the primary financial metric.
        /// </summary>
        public decimal AvailableBudget { get; set; }

        /// <summary>
        /// The total revenue/income that the company has generated to date.
        /// Represents the gross financial capacity before expenses are deducted.
        /// </summary>
        public decimal CurrentRevenue { get; set; }

        /// <summary>
        /// The cumulative amount spent on orders, purchases, and operational expenses.
        /// Used to calculate remaining budget and track spending patterns over time.
        /// </summary>
        public decimal TotalSpent { get; set; }

        /// <summary>
        /// The total quantity of all items currently in inventory across all warehouses.
        /// Provides a quick overview of overall stock levels for operational planning.
        /// </summary>
        public int TotalStock { get; set; }

        /// <summary>
        /// The number of product requests that are awaiting approval from managers.
        /// Indicates workflow backlog and helps prioritize administrative tasks.
        /// </summary>
        public int PendingRequests { get; set; }

        /// <summary>
        /// The timestamp when the financial data was last updated in the system.
        /// Helps administrators understand the freshness of the displayed information.
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets a user-friendly formatted string representation of the available budget.
        /// Automatically formats large numbers with K (thousands) or M (millions) suffixes
        /// for better readability in the dashboard interface.
        /// </summary>
        /// <example>
        /// $1,500,000 becomes "$1.5M"
        /// $850,000 becomes "$850K"
        /// $500 becomes "$500"
        /// </example>
        public string AvailableBudgetDisplay => FormatCurrency(AvailableBudget);

        /// <summary>
        /// Gets a user-friendly formatted string representation of the current revenue.
        /// Uses the same formatting logic as AvailableBudgetDisplay for consistency
        /// across all financial displays in the dashboard.
        /// </summary>
        public string CurrentRevenueDisplay => FormatCurrency(CurrentRevenue);

        /// <summary>
        /// Gets a user-friendly formatted string representation of the total spent amount.
        /// Provides consistent formatting with other financial metrics for a cohesive
        /// user experience in the admin dashboard.
        /// </summary>
        public string TotalSpentDisplay => FormatCurrency(TotalSpent);

        /// <summary>
        /// Formats decimal currency values into human-readable strings with appropriate suffixes.
        /// Converts large numbers into abbreviated format (K for thousands, M for millions)
        /// to improve readability and save space in the dashboard interface.
        /// </summary>
        /// <param name="amount">The decimal amount to format</param>
        /// <returns>
        /// A formatted string with currency symbol and appropriate suffix:
        /// - Amounts >= 1,000,000: formatted as "$X.XM" (millions)
        /// - Amounts >= 1,000: formatted as "$XXXK" (thousands)  
        /// - Amounts < 1,000: formatted as "$XXX" (exact amount)
        /// </returns>
        /// <example>
        /// FormatCurrency(1500000m) returns "$1.5M"
        /// FormatCurrency(25000m) returns "$25K"
        /// FormatCurrency(750m) returns "$750"
        /// </example>
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