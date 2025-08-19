using System;
namespace LogisticsPro.UI.Models
{
    public class Product
    {
        
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public string SKU { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public string UnitOfMeasure { get; set; } = null!;
        public string Supplier { get; set; } = null!;
        public string Status { get; set; } = "Active";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        
        // Default constructor
        public Product()
        {
        }
        
        // Parameterized constructor
        public Product(int id, string name, string sku, string category, decimal unitPrice,
            string unitOfMeasure, string supplier, string? description = null,
            string status = "Active", DateTime? createdDate = null)
        {
            Id = id;
            Name = name;
            Description = description;
            SKU = sku;
            Category = category;
            UnitPrice = unitPrice;
            UnitOfMeasure = unitOfMeasure;
            Supplier = supplier;
            Status = status;
            CreatedDate = createdDate ?? DateTime.Now;
        }

    }
}