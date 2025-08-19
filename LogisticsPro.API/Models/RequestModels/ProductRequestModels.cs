using System.ComponentModel.DataAnnotations;

namespace LogisticsPro.API.Models.RequestModels
{
    public class CreateProductRequestRequest
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int RequestedQuantity { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RequestedBy { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class UpdateStatusRequest
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class UpdateProductRequestRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int? RequestedQuantity { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    // New Status Management DTOs
    public class ApproveRequestModel
    {
        [Required]
        [StringLength(50)]
        public string ApprovedBy { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class RejectRequestModel
    {
        [Required]
        [StringLength(50)]
        public string RejectedBy { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000, ErrorMessage = "Rejection reason is required")]
        public string? Notes { get; set; }
    }

    public class CancelRequestModel
    {
        [Required]
        [StringLength(50)]
        public string CancelledBy { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class ReceiveRequestModel
    {
        [Required]
        [StringLength(50)]
        public string ReceivedBy { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}