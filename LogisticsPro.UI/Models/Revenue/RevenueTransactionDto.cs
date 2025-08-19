using System;

namespace LogisticsPro.UI.Models.Revenue;

public class RevenueTransactionDto
{
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int? ProductRequestId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal BalanceAfter { get; set; }
}