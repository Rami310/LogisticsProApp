using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogisticsPro.API.Models
{
    public class RevenueTransaction
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string TransactionType { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        
        public int? ProductRequestId { get; set; }
        
        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        [MaxLength(255)]
        public string? Description { get; set; }
        
        [Column(TypeName = "decimal(15,2)")]
        public decimal BalanceAfter { get; set; }
        
        // Foreign key to ProductRequests
        [ForeignKey("ProductRequestId")]
        public ProductRequest? ProductRequest { get; set; }
    }
}