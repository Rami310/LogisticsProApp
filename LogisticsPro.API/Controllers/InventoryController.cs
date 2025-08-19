using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticsPro.API.Data;
using LogisticsPro.API.Models;

namespace LogisticsPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly LogisticsDbContext _context;

        public InventoryController(LogisticsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetInventoryItems()
        {
            try
            {
                var inventoryItems = await _context.InventoryItems
                    .Join(_context.Products,
                        inventory => inventory.ProductId,
                        product => product.Id,
                        (inventory, product) => new
                        {
                            id = inventory.Id,
                            productId = inventory.ProductId,
                            warehouseId = inventory.WarehouseId,
                            location = inventory.Location,
                            quantityInStock = inventory.QuantityInStock,
                            minimumStockLevel = inventory.MinimumStockLevel,
                            maximumStockLevel = inventory.MaximumStockLevel,
                            lastStockUpdate = inventory.LastStockUpdate,
                            product = new
                            {
                                id = product.Id,
                                name = product.Name,
                                description = product.Description,
                                sku = product.SKU,
                                category = product.Category,
                                unitPrice = product.UnitPrice,
                                unitOfMeasure = product.UnitOfMeasure,
                                supplier = product.Supplier,
                                status = product.Status
                            }
                        })
                    .ToListAsync();

                Console.WriteLine($"Retrieved {inventoryItems.Count} inventory items");
                return Ok(inventoryItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting inventory: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving inventory items", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetInventoryItem(int id)
        {
            try
            {
                var inventoryItem = await _context.InventoryItems
                    .Where(i => i.Id == id)
                    .Join(_context.Products,
                        inventory => inventory.ProductId,
                        product => product.Id,
                        (inventory, product) => new
                        {
                            id = inventory.Id,
                            productId = inventory.ProductId,
                            warehouseId = inventory.WarehouseId,
                            location = inventory.Location,
                            quantityInStock = inventory.QuantityInStock,
                            minimumStockLevel = inventory.MinimumStockLevel,
                            maximumStockLevel = inventory.MaximumStockLevel,
                            lastStockUpdate = inventory.LastStockUpdate,
                            product = new
                            {
                                id = product.Id,
                                name = product.Name,
                                description = product.Description,
                                sku = product.SKU,
                                category = product.Category,
                                unitPrice = product.UnitPrice,
                                supplier = product.Supplier
                            }
                        })
                    .FirstOrDefaultAsync();

                if (inventoryItem == null)
                {
                    return NotFound(new { message = $"Inventory item with ID {id} not found" });
                }

                return Ok(inventoryItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting inventory item {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving inventory item", error = ex.Message });
            }
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<object>> GetInventoryByProduct(int productId)
        {
            try
            {
                var inventoryItem = await _context.InventoryItems
                    .Where(i => i.ProductId == productId)
                    .Join(_context.Products,
                        inventory => inventory.ProductId,
                        product => product.Id,
                        (inventory, product) => new
                        {
                            id = inventory.Id,
                            productId = inventory.ProductId,
                            location = inventory.Location,
                            quantityInStock = inventory.QuantityInStock,
                            minimumStockLevel = inventory.MinimumStockLevel,
                            maximumStockLevel = inventory.MaximumStockLevel,
                            lastStockUpdate = inventory.LastStockUpdate,
                            product = new
                            {
                                id = product.Id,
                                name = product.Name,
                                sku = product.SKU,
                                category = product.Category
                            }
                        })
                    .FirstOrDefaultAsync();

                if (inventoryItem == null)
                {
                    return NotFound(new { message = $"No inventory found for product ID {productId}" });
                }

                return Ok(inventoryItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting inventory for product {productId}: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving inventory by product", error = ex.Message });
            }
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<object>>> GetLowStockItems()
        {
            try
            {
                var lowStockItems = await _context.InventoryItems
                    .Where(i => i.QuantityInStock <= i.MinimumStockLevel)
                    .Join(_context.Products,
                        inventory => inventory.ProductId,
                        product => product.Id,
                        (inventory, product) => new
                        {
                            id = inventory.Id,
                            productId = inventory.ProductId,
                            location = inventory.Location,
                            quantityInStock = inventory.QuantityInStock,
                            minimumStockLevel = inventory.MinimumStockLevel,
                            maximumStockLevel = inventory.MaximumStockLevel,
                            lastStockUpdate = inventory.LastStockUpdate,
                            product = new
                            {
                                id = product.Id,
                                name = product.Name,
                                sku = product.SKU,
                                category = product.Category,
                                supplier = product.Supplier
                            }
                        })
                    .ToListAsync();

                Console.WriteLine($"Found {lowStockItems.Count} low stock items");
                return Ok(lowStockItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting low stock items: {ex.Message}");
                return StatusCode(500, new { message = "Error retrieving low stock items", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<InventoryItem>> CreateInventoryItem(CreateInventoryRequest request)
        {
            try
            {
                var productExists = await _context.Products.AnyAsync(p => p.Id == request.ProductId);
                if (!productExists)
                {
                    return BadRequest(new { message = "Product not found" });
                }

                var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == request.WarehouseId);
                if (!warehouseExists)
                {
                    return BadRequest(new { message = "Warehouse not found" });
                }

                var inventoryItem = new InventoryItem
                {
                    ProductId = request.ProductId,
                    WarehouseId = request.WarehouseId,
                    Location = request.Location,
                    QuantityInStock = request.QuantityInStock,
                    MinimumStockLevel = request.MinimumStockLevel,
                    MaximumStockLevel = request.MaximumStockLevel,
                    LastStockUpdate = DateTime.Now
                };

                _context.InventoryItems.Add(inventoryItem);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Created inventory item for product {request.ProductId}");
                return CreatedAtAction(nameof(GetInventoryItem), new { id = inventoryItem.Id }, inventoryItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating inventory item: {ex.Message}");
                return StatusCode(500, new { message = "Error creating inventory item", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventoryItem(int id, UpdateInventoryRequest request)
        {
            try
            {
                var inventoryItem = await _context.InventoryItems.FindAsync(id);
                if (inventoryItem == null)
                {
                    return NotFound(new { message = $"Inventory item with ID {id} not found" });
                }

                inventoryItem.Location = request.Location ?? inventoryItem.Location;
                inventoryItem.QuantityInStock = request.QuantityInStock ?? inventoryItem.QuantityInStock;
                inventoryItem.MinimumStockLevel = request.MinimumStockLevel ?? inventoryItem.MinimumStockLevel;
                inventoryItem.MaximumStockLevel = request.MaximumStockLevel ?? inventoryItem.MaximumStockLevel;
                inventoryItem.LastStockUpdate = DateTime.Now;

                await _context.SaveChangesAsync();

                Console.WriteLine($"Updated inventory item {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating inventory item {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error updating inventory item", error = ex.Message });
            }
        }

        [HttpPut("stock")]
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockRequest request)
        {
            try
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == request.ProductId);

                if (inventoryItem == null)
                {
                    return NotFound(new { message = "Inventory item not found for this product" });
                }

                if (request.IsAddition)
                {
                    inventoryItem.QuantityInStock += request.Quantity;
                }
                else
                {
                    if (inventoryItem.QuantityInStock < request.Quantity)
                    {
                        return BadRequest(new { message = "Insufficient stock" });
                    }
                    inventoryItem.QuantityInStock -= request.Quantity;
                }

                inventoryItem.LastStockUpdate = DateTime.Now;
                await _context.SaveChangesAsync();

                Console.WriteLine($"Updated stock for product {request.ProductId}: {inventoryItem.QuantityInStock}");
                return Ok(new { message = "Stock updated successfully", newQuantity = inventoryItem.QuantityInStock });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating stock: {ex.Message}");
                return StatusCode(500, new { message = "Error updating stock", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryItem(int id)
        {
            try
            {
                var inventoryItem = await _context.InventoryItems.FindAsync(id);
                if (inventoryItem == null)
                {
                    return NotFound(new { message = $"Inventory item with ID {id} not found" });
                }

                _context.InventoryItems.Remove(inventoryItem);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Deleted inventory item {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting inventory item {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error deleting inventory item", error = ex.Message });
            }
        }
    }

    public class CreateInventoryRequest
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; } = 1;
        public string Location { get; set; } = string.Empty;
        public int QuantityInStock { get; set; }
        public int MinimumStockLevel { get; set; }
        public int MaximumStockLevel { get; set; }
    }

    public class UpdateInventoryRequest
    {
        public string? Location { get; set; }
        public int? QuantityInStock { get; set; }
        public int? MinimumStockLevel { get; set; }
        public int? MaximumStockLevel { get; set; }
    }

    public class UpdateStockRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public bool IsAddition { get; set; } = true;
    }
}