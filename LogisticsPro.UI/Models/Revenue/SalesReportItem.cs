using System;

namespace LogisticsPro.UI.Models.Revenue
{
    /// <summary>
    /// Individual sales report item for recent sales table
    /// </summary>
    public class SalesReportItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Profit { get; set; }
        public DateTime SaleDate { get; set; }
        public string SoldBy { get; set; } = "";
        
        public decimal ProfitMargin => Cost > 0 ? (Profit / Cost) * 100 : 0;
    }
}