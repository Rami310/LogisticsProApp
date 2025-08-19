using System;
namespace LogisticsPro.API.Models
{
    public class Product
    {
        
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public string? Category { get; set; }
        public decimal UnitPrice { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? Supplier { get; set; }
        public string? Status { get; set; } = "Active";
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