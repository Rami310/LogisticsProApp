using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogisticsPro.API.Models
{
    public class CompanyRevenue
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal CurrentRevenue { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal AvailableBudget { get; set; }
        
        [Column(TypeName = "decimal(15,2)")]
        public decimal TotalSpent { get; set; } = 0;
        
        // Use DateTime instead of TIMESTAMP to avoid MySQL version issues
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        [MaxLength(50)]
        public string? UpdatedBy { get; set; }
        
        [MaxLength(255)]
        public string? UpdateReason { get; set; }
    }
}