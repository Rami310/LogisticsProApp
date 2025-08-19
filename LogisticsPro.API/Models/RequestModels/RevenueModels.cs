using System.ComponentModel.DataAnnotations;

namespace LogisticsPro.API.Models.RequestModels
{
    public class DeductRevenueRequest
    {
        public int? ProductRequestId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [StringLength(50)]
        public string? TransactionType { get; set; } = "ORDER_PLACED";
        
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        
        [StringLength(255)]
        public string? Description { get; set; }
    }

    public class RestoreRevenueRequest
    {
        public int? ProductRequestId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [StringLength(50)]
        public string? TransactionType { get; set; } = "ORDER_CANCELLED";
        
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        
        [StringLength(255)]
        public string? Description { get; set; }
    }

    public class AdjustRevenueRequest
    {
        [Range(0, double.MaxValue, ErrorMessage = "Revenue cannot be negative")]
        public decimal? NewCurrentRevenue { get; set; }
        
        public decimal? BudgetAdjustment { get; set; }
        
        [StringLength(50)]
        public string? UpdatedBy { get; set; }
        
        [Required]
        [StringLength(255)]
        public string? Reason { get; set; }
    }
}