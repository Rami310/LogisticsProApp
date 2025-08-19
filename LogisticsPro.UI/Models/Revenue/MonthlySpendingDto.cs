namespace LogisticsPro.UI.Models.Revenue;

public class MonthlySpendingDto
{
    public decimal CurrentMonthSpent { get; set; }
    public decimal PreviousMonthSpent { get; set; }
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
}