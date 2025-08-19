using System;

namespace LogisticsPro.API.Models;

public class InventoryItem
{

    public int Id { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; } = 1;
    public string Location { get; set; } = null!;
    public int QuantityInStock { get; set; }
    public int MinimumStockLevel { get; set; }
    public int MaximumStockLevel { get; set; }
    public DateTime LastStockUpdate { get; set; } = DateTime.Now;


    // Default constructor
    public InventoryItem()
    {
    }

    // Parameterized constructor
    public InventoryItem(int id, int productId, string location, int quantityInStock,
        int minimumStockLevel, int maximumStockLevel,
        int warehouseId = 1, DateTime? lastStockUpdate = null)
    {
        Id = id;
        ProductId = productId;
        WarehouseId = warehouseId;
        Location = location;
        QuantityInStock = quantityInStock;
        MinimumStockLevel = minimumStockLevel;
        MaximumStockLevel = maximumStockLevel;
        LastStockUpdate = lastStockUpdate ?? DateTime.Now;
    }
}