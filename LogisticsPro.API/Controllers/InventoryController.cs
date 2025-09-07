using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticsPro.API.Data;
using LogisticsPro.API.Models;

namespace LogisticsPro.API.Controllers
{
    /// <summary>
    /// API controller for managing inventory items and stock levels.
    /// Provides CRUD operations for inventory management including stock updates,
    /// low stock alerts, and inventory item queries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly LogisticsDbContext _context;

        /// <summary>
        /// Initializes a new instance of the InventoryController.
        /// </summary>
        /// <param name="context">The database context for accessing inventory data</param>
        public InventoryController(LogisticsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all inventory items with their associated product information.
        /// </summary>
        /// <returns>A list of inventory items including product details, stock levels, and location information</returns>
        /// <response code="200">Returns the list of inventory items</response>
        /// <response code="500">If there was an internal server error</response>
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

        /// <summary>
        /// Retrieves a specific inventory item by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the inventory item</param>
        /// <returns>The inventory item with associated product information</returns>
        /// <response code="200">Returns the inventory item</response>
        /// <response code="404">If the inventory item is not found</response>
        /// <response code="500">If there was an internal server error</response>
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

        /// <summary>
        /// Retrieves inventory information for a specific product.
        /// </summary>
        /// <param name="productId">The unique identifier of the product</param>
        /// <returns>The inventory item associated with the specified product</returns>
        /// <response code="200">Returns the inventory item for the product</response>
        /// <response code="404">If no inventory is found for the product</response>
        /// <response code="500">If there was an internal server error</response>
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

        /// <summary>
        /// Retrieves all inventory items that have stock levels at or below their minimum threshold.
        /// Used for generating low stock alerts and reorder recommendations.
        /// </summary>
        /// <returns>A list of inventory items with low stock levels</returns>
        /// <response code="200">Returns the list of low stock items</response>
        /// <response code="500">If there was an internal server error</response>
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

        /// <summary>
        /// Creates a new inventory item for a product in a warehouse.
        /// </summary>
        /// <param name="request">The inventory item creation request containing product ID, warehouse ID, and stock levels</param>
        /// <returns>The created inventory item</returns>
        /// <response code="201">Returns the newly created inventory item</response>
        /// <response code="400">If the product or warehouse doesn't exist</response>
        /// <response code="500">If there was an internal server error</response>
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

        /// <summary>
        /// Updates an existing inventory item's properties (excluding stock quantity).
        /// Use the UpdateStock endpoint for stock quantity changes.
        /// </summary>
        /// <param name="id">The unique identifier of the inventory item to update</param>
        /// <param name="request">The update request containing the new values for inventory properties</param>
        /// <returns>No content on successful update</returns>
        /// <response code="204">Inventory item updated successfully</response>
        /// <response code="404">If the inventory item is not found</response>
        /// <response code="500">If there was an internal server error</response>
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

        /// <summary>
        /// Updates the stock quantity for a product by adding or subtracting a specified amount.
        /// Validates that sufficient stock exists before allowing deductions.
        /// </summary>
        /// <param name="request">The stock update request containing product ID, quantity, and operation type</param>
        /// <returns>Success message with new stock quantity</returns>
        /// <response code="200">Stock updated successfully</response>
        /// <response code="400">If insufficient stock for deduction</response>
        /// <response code="404">If the inventory item is not found</response>
        /// <response code="500">If there was an internal server error</response>
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

        /// <summary>
        /// Deletes an inventory item from the system.
        /// Use with caution as this will permanently remove the inventory record.
        /// </summary>
        /// <param name="id">The unique identifier of the inventory item to delete</param>
        /// <returns>No content on successful deletion</returns>
        /// <response code="204">Inventory item deleted successfully</response>
        /// <response code="404">If the inventory item is not found</response>
        /// <response code="500">If there was an internal server error</response>
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

    /// <summary>
    /// Request model for creating a new inventory item.
    /// </summary>
    public class CreateInventoryRequest
    {
        /// <summary>
        /// The unique identifier of the product for this inventory item.
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// The unique identifier of the warehouse where the product is stored.
        /// Defaults to warehouse ID 1 if not specified.
        /// </summary>
        public int WarehouseId { get; set; } = 1;
        
        /// <summary>
        /// The specific location within the warehouse (e.g., "A1-B2", "Section C").
        /// </summary>
        public string Location { get; set; } = string.Empty;
        
        /// <summary>
        /// The current quantity of the product in stock.
        /// </summary>
        public int QuantityInStock { get; set; }
        
        /// <summary>
        /// The minimum stock level that triggers reorder alerts.
        /// </summary>
        public int MinimumStockLevel { get; set; }
        
        /// <summary>
        /// The maximum stock level that should not be exceeded.
        /// </summary>
        public int MaximumStockLevel { get; set; }
    }

    /// <summary>
    /// Request model for updating an existing inventory item's properties.
    /// All properties are optional - only provided values will be updated.
    /// </summary>
    public class UpdateInventoryRequest
    {
        /// <summary>
        /// The new location within the warehouse. If null, current location is preserved.
        /// </summary>
        public string? Location { get; set; }
        
        /// <summary>
        /// The new quantity in stock. If null, current quantity is preserved.
        /// For stock adjustments, use the UpdateStock endpoint instead.
        /// </summary>
        public int? QuantityInStock { get; set; }
        
        /// <summary>
        /// The new minimum stock level. If null, current level is preserved.
        /// </summary>
        public int? MinimumStockLevel { get; set; }
        
        /// <summary>
        /// The new maximum stock level. If null, current level is preserved.
        /// </summary>
        public int? MaximumStockLevel { get; set; }
    }

    /// <summary>
    /// Request model for updating stock quantities through addition or subtraction.
    /// </summary>
    public class UpdateStockRequest
    {
        /// <summary>
        /// The unique identifier of the product whose stock should be updated.
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// The quantity to add or subtract from current stock.
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Whether this is an addition (true) or subtraction (false) operation.
        /// Defaults to true (addition).
        /// </summary>
        public bool IsAddition { get; set; } = true;
    }
}