namespace LogisticsPro.API.Controllers;

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