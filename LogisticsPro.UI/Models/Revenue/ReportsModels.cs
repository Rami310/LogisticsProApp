

using System;

namespace LogisticsPro.UI.Models.Revenue
{
    /// <summary>
    /// Simple statistics summary
    /// </summary>
    public class RevenueStatisticsDto
    {
        public decimal TotalProfit { get; set; }
        public decimal CurrentMonthProfit { get; set; }
        public decimal LastMonthProfit { get; set; }
        public int CurrentMonthTransactions { get; set; }
        public int LastMonthTransactions { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Monthly trend for charts
    /// </summary>
    public class MonthlyProfitTrendDto
    {
        public string Month { get; set; } = "";
        public decimal Profit { get; set; }
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// Product profit analysis
    /// </summary>
    public class ProductProfitApiDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal TotalProfit { get; set; }
        public int TransactionCount { get; set; }
    }
}