using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogisticsPro.API.Models
{
    public class ProductRequest
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        
        public int RequestedQuantity { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string RequestedBy { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string RequestStatus { get; set; } = "Pending";
        
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime? ApprovalDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        
        [MaxLength(50)]
        public string? ApprovedBy { get; set; }
        
        [MaxLength(50)]
        public string? ReceivedBy { get; set; }
        
        [MaxLength(1000)]
        public string? Notes { get; set; }
        
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCost { get; set; } = 0;

        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        
        // Navigation property
        public Product? Product { get; set; }

        // Default constructor
        public ProductRequest()
        {
        }

        // Simplified constructor
        public ProductRequest(int productId, int requestedQuantity, string requestedBy)
        {
            ProductId = productId;
            RequestedQuantity = requestedQuantity;
            RequestedBy = requestedBy;
            RequestDate = DateTime.Now;
        }
    }
}