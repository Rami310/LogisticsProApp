using System;

namespace LogisticsPro.UI.Models.Revenue
{
    public class CompanyRevenueDto
    {
        public decimal CurrentRevenue { get; set; }
        public decimal AvailableBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastUpdated { get; set; }
    }
    
}