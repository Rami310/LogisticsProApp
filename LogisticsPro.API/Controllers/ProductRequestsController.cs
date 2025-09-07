/*
 * ProductRequestsController.cs
 * 
 * Purpose: Manages product requests throughout their lifecycle (create, approve, reject, cancel, receive)
 * Dependencies: LogisticsDbContext (DB), HttpClient (revenue API calls)
 * 
 * Key Endpoints:
 * - GET /api/ProductRequests - List all requests with product details
 * - GET /api/ProductRequests/status/{status} - Filter by status
 * - GET /api/ProductRequests/user/{username} - Filter by user
 * - POST /api/ProductRequests - Create new request
 * - PUT /api/ProductRequests/{id}/approve - Approve pending request
 * - PUT /api/ProductRequests/{id}/reject - Reject pending request
 * - PUT /api/ProductRequests/{id}/cancel - Cancel request
 * - PUT /api/ProductRequests/{id}/receive - Mark approved as received
 * - DELETE /api/ProductRequests/{id} - Delete pending requests only
 * 
 * Features: Auto cost calculation, inventory updates, revenue restoration, status validation
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticsPro.API.Data;
using LogisticsPro.API.Models;
using LogisticsPro.API.Models.RequestModels;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace LogisticsPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductRequestsController : ControllerBase
    {
        private readonly LogisticsDbContext _context;
        private readonly HttpClient _httpClient;

        public ProductRequestsController(LogisticsDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProductRequests()
        {
            var requests = await _context.ProductRequests
                .Include(r => r.Product)
                .Select(r => new
                {
                    id = r.Id,
                    productId = r.ProductId,
                    requestedQuantity = r.RequestedQuantity,
                    requestedBy = r.RequestedBy,
                    requestStatus = r.RequestStatus,
                    requestDate = r.RequestDate,
                    approvalDate = r.ApprovalDate,
                    receivedDate = r.ReceivedDate,
                    approvedBy = r.ApprovedBy,
                    receivedBy = r.ReceivedBy,
                    notes = r.Notes,
                    totalCost = r.TotalCost,
                    createdBy = r.CreatedBy,
                    product = new
                    {
                        id = r.Product.Id,
                        name = r.Product.Name,
                        description = r.Product.Description,
                        sku = r.Product.SKU,
                        category = r.Product.Category,
                        unitPrice = r.Product.UnitPrice,
                        supplier = r.Product.Supplier
                    }
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductRequest>> GetProductRequest(int id)
        {
            var productRequest = await _context.ProductRequests
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (productRequest == null)
            {
                return NotFound();
            }

            return Ok(productRequest);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRequestsByStatus(string status)
        {
            try
            {
                var requests = await _context.ProductRequests
                    .Where(r => r.RequestStatus.ToLower() == status.ToLower())
                    .ToListAsync();

                var productIds = requests.Select(r => r.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                var result = requests.Select(r => new
                {
                    id = r.Id,
                    productId = r.ProductId,
                    requestedQuantity = r.RequestedQuantity,
                    requestedBy = r.RequestedBy,
                    requestStatus = r.RequestStatus,
                    requestDate = r.RequestDate,
                    approvalDate = r.ApprovalDate,
                    receivedDate = r.ReceivedDate,
                    approvedBy = r.ApprovedBy,
                    receivedBy = r.ReceivedBy,
                    notes = r.Notes,
                    totalCost = r.TotalCost,
                    createdBy = r.CreatedBy,
                    product = products.FirstOrDefault(p => p.Id == r.ProductId) == null
                        ? null
                        : new
                        {
                            id = products.FirstOrDefault(p => p.Id == r.ProductId)!.Id,
                            name = products.FirstOrDefault(p => p.Id == r.ProductId)!.Name ?? "Unknown",
                            sku = products.FirstOrDefault(p => p.Id == r.ProductId)!.SKU ?? "Unknown",
                            category = products.FirstOrDefault(p => p.Id == r.ProductId)!.Category ?? "N/A",
                            supplier = products.FirstOrDefault(p => p.Id == r.ProductId)!.Supplier ?? "N/A",
                            unitPrice = products.FirstOrDefault(p => p.Id == r.ProductId)!.UnitPrice
                        }
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error in GetRequestsByStatus: {ex.Message}");
                return StatusCode(500, new { error = "Server error", details = ex.Message });
            }
        }

        [HttpGet("user/{username}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRequestsByUser(string username)
        {
            var requests = await _context.ProductRequests
                .Include(r => r.Product)
                .Where(r => r.RequestedBy.ToLower() == username.ToLower())
                .Select(r => new
                {
                    id = r.Id,
                    productId = r.ProductId,
                    requestedQuantity = r.RequestedQuantity,
                    requestedBy = r.RequestedBy,
                    requestStatus = r.RequestStatus,
                    requestDate = r.RequestDate,
                    approvalDate = r.ApprovalDate,
                    receivedDate = r.ReceivedDate,
                    approvedBy = r.ApprovedBy,
                    receivedBy = r.ReceivedBy,
                    notes = r.Notes,
                    totalCost = r.TotalCost,
                    createdBy = r.CreatedBy,
                    product = new
                    {
                        id = r.Product.Id,
                        name = r.Product.Name,
                        sku = r.Product.SKU,
                        category = r.Product.Category,
                        unitPrice = r.Product.UnitPrice
                    }
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost]
        public async Task<ActionResult<ProductRequest>> CreateProductRequest(CreateProductRequestRequest request)
        {
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                return BadRequest(new { message = "Product not found" });
            }

            var totalCost = product.UnitPrice * request.RequestedQuantity;

            var productRequest = new ProductRequest
            {
                ProductId = request.ProductId,
                RequestedQuantity = request.RequestedQuantity,
                RequestedBy = request.RequestedBy,
                RequestStatus = "Pending",
                RequestDate = DateTime.Now,
                Notes = request.Notes,
                TotalCost = totalCost,
                CreatedBy = request.RequestedBy
            };

            _context.ProductRequests.Add(productRequest);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Product request created - ID: {productRequest.Id}, Cost: ${totalCost:F2}");
            return CreatedAtAction(nameof(GetProductRequest), new { id = productRequest.Id }, productRequest);
        }

        [HttpPut("{id}/approve")]
        public async Task<ActionResult<object>> ApproveRequest(int id, [FromBody] ApproveRequestModel request)
        {
            try
            {
                var productRequest = await _context.ProductRequests
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (productRequest == null)
                {
                    return NotFound(new { message = "Product request not found" });
                }

                if (productRequest.RequestStatus.ToLower() != "pending")
                {
                    return BadRequest(new
                        { message = $"Cannot approve request with status: {productRequest.RequestStatus}" });
                }

                var oldStatus = productRequest.RequestStatus;

                productRequest.RequestStatus = "Approved";
                productRequest.ApprovedBy = request.ApprovedBy;
                productRequest.ApprovalDate = DateTime.Now;

                if (!string.IsNullOrEmpty(request.Notes))
                {
                    productRequest.Notes = string.IsNullOrEmpty(productRequest.Notes)
                        ? request.Notes
                        : $"{productRequest.Notes}\n[APPROVED] {request.Notes}";
                }

                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == productRequest.ProductId);

                bool inventoryUpdated = false;
                if (inventoryItem != null)
                {
                    inventoryItem.QuantityInStock += productRequest.RequestedQuantity;
                    inventoryItem.LastStockUpdate = DateTime.Now;
                    inventoryUpdated = true;
                    Console.WriteLine($"Inventory updated - Product {productRequest.ProductId}: +{productRequest.RequestedQuantity} units");
                }
                else
                {
                    Console.WriteLine($"Warning: No inventory item found for Product {productRequest.ProductId}");
                }

                await _context.SaveChangesAsync();

                var result = new
                {
                    success = true,
                    message = $"Request {id} approved and inventory updated successfully",
                    requestId = id,
                    oldStatus = oldStatus,
                    newStatus = productRequest.RequestStatus,
                    approvedBy = productRequest.ApprovedBy,
                    approvalDate = productRequest.ApprovalDate,
                    productName = productRequest.Product?.Name,
                    totalCost = productRequest.TotalCost,
                    inventoryUpdated = inventoryUpdated,
                    quantityAdded = productRequest.RequestedQuantity
                };

                Console.WriteLine($"Request {id} approved by {request.ApprovedBy} - Cost: ${productRequest.TotalCost:F2}, Inventory: +{productRequest.RequestedQuantity}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving request {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to approve request", details = ex.Message });
            }
        }

        [HttpPut("{id}/reject")]
        public async Task<ActionResult<object>> RejectRequest(int id, [FromBody] RejectRequestModel request)
        {
            try
            {
                var productRequest = await _context.ProductRequests
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (productRequest == null)
                {
                    return NotFound(new { message = "Product request not found" });
                }

                if (productRequest.RequestStatus.ToLower() != "pending")
                {
                    return BadRequest(new
                        { message = $"Cannot reject request with status: {productRequest.RequestStatus}" });
                }

                var oldStatus = productRequest.RequestStatus;
                var totalCost = productRequest.TotalCost;

                productRequest.RequestStatus = "Rejected";
                productRequest.ApprovedBy = request.RejectedBy;
                productRequest.ApprovalDate = DateTime.Now;

                if (!string.IsNullOrEmpty(request.Notes))
                {
                    productRequest.Notes = string.IsNullOrEmpty(productRequest.Notes)
                        ? $"[REJECTED] {request.Notes}"
                        : $"{productRequest.Notes}\n[REJECTED] {request.Notes}";
                }

                await _context.SaveChangesAsync();

                bool revenueRestored =
                    await RestoreRevenueAsync(productRequest.Id, totalCost, request.RejectedBy, "ORDER_REJECTED");

                var result = new
                {
                    success = true,
                    message = $"Request {id} rejected successfully",
                    requestId = id,
                    oldStatus = oldStatus,
                    newStatus = productRequest.RequestStatus,
                    rejectedBy = productRequest.ApprovedBy,
                    rejectionDate = productRequest.ApprovalDate,
                    productName = productRequest.Product?.Name,
                    totalCost = totalCost,
                    revenueRestored = revenueRestored
                };

                Console.WriteLine(
                    $"Request {id} rejected by {request.RejectedBy} - Revenue restored: ${totalCost:F2}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rejecting request {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to reject request", details = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<object>> CancelRequest(int id, [FromBody] CancelRequestModel request)
        {
            try
            {
                var productRequest = await _context.ProductRequests
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (productRequest == null)
                {
                    return NotFound(new { message = "Product request not found" });
                }

                if (productRequest.RequestStatus.ToLower() == "received")
                {
                    return BadRequest(new { message = "Cannot cancel a request that has already been received" });
                }

                var oldStatus = productRequest.RequestStatus;
                var totalCost = productRequest.TotalCost;

                productRequest.RequestStatus = "Cancelled";

                if (!string.IsNullOrEmpty(request.Notes))
                {
                    productRequest.Notes = string.IsNullOrEmpty(productRequest.Notes)
                        ? $"[CANCELLED] {request.Notes}"
                        : $"{productRequest.Notes}\n[CANCELLED] {request.Notes}";
                }

                await _context.SaveChangesAsync();

                bool revenueRestored = false;
                if (oldStatus.ToLower() == "pending" || oldStatus.ToLower() == "approved")
                {
                    revenueRestored = await RestoreRevenueAsync(productRequest.Id, totalCost, request.CancelledBy,
                        "ORDER_CANCELLED");
                }

                var result = new
                {
                    success = true,
                    message = $"Request {id} cancelled successfully",
                    requestId = id,
                    oldStatus = oldStatus,
                    newStatus = productRequest.RequestStatus,
                    cancelledBy = request.CancelledBy,
                    cancellationDate = DateTime.Now,
                    productName = productRequest.Product?.Name,
                    totalCost = totalCost,
                    revenueRestored = revenueRestored
                };

                Console.WriteLine(
                    $"Request {id} cancelled by {request.CancelledBy} - Revenue restored: ${(revenueRestored ? totalCost : 0):F2}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling request {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to cancel request", details = ex.Message });
            }
        }

        [HttpPut("{id}/receive")]
        public async Task<ActionResult<object>> ReceiveRequest(int id, [FromBody] ReceiveRequestModel request)
        {
            try
            {
                var productRequest = await _context.ProductRequests
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (productRequest == null)
                {
                    return NotFound(new { message = "Product request not found" });
                }

                if (productRequest.RequestStatus.ToLower() != "approved")
                {
                    return BadRequest(new
                    {
                        message =
                            $"Cannot receive request with status: {productRequest.RequestStatus}. Must be approved first."
                    });
                }

                var oldStatus = productRequest.RequestStatus;

                productRequest.RequestStatus = "Received";
                productRequest.ReceivedBy = request.ReceivedBy;
                productRequest.ReceivedDate = DateTime.Now;

                if (!string.IsNullOrEmpty(request.Notes))
                {
                    productRequest.Notes = string.IsNullOrEmpty(productRequest.Notes)
                        ? $"[RECEIVED] {request.Notes}"
                        : $"{productRequest.Notes}\n[RECEIVED] {request.Notes}";
                }

                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == productRequest.ProductId);

                if (inventoryItem != null)
                {
                    inventoryItem.QuantityInStock += productRequest.RequestedQuantity;
                    inventoryItem.LastStockUpdate = DateTime.Now;
                    Console.WriteLine(
                        $"Inventory updated - Product {productRequest.ProductId}: +{productRequest.RequestedQuantity} units");
                }
                else
                {
                    Console.WriteLine($"Warning: No inventory item found for Product {productRequest.ProductId}");
                }

                await _context.SaveChangesAsync();

                var result = new
                {
                    success = true,
                    message = $"Request {id} marked as received successfully",
                    requestId = id,
                    oldStatus = oldStatus,
                    newStatus = productRequest.RequestStatus,
                    receivedBy = productRequest.ReceivedBy,
                    receivedDate = productRequest.ReceivedDate,
                    productName = productRequest.Product?.Name,
                    quantityReceived = productRequest.RequestedQuantity,
                    totalCost = productRequest.TotalCost,
                    inventoryUpdated = inventoryItem != null
                };

                Console.WriteLine(
                    $"Request {id} received by {request.ReceivedBy} - {productRequest.RequestedQuantity} units added to inventory");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving request {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to receive request", details = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var productRequest = await _context.ProductRequests.FindAsync(id);
            if (productRequest == null)
            {
                return NotFound();
            }

            var oldStatus = productRequest.RequestStatus;
            productRequest.RequestStatus = request.Status;

            switch (request.Status.ToLower())
            {
                case "approved":
                    productRequest.ApprovedBy = request.UpdatedBy;
                    productRequest.ApprovalDate = DateTime.Now;
                    break;

                case "received":
                    productRequest.ReceivedBy = request.UpdatedBy;
                    productRequest.ReceivedDate = DateTime.Now;

                    var inventoryItem = await _context.InventoryItems
                        .FirstOrDefaultAsync(i => i.ProductId == productRequest.ProductId);

                    if (inventoryItem != null)
                    {
                        inventoryItem.QuantityInStock += productRequest.RequestedQuantity;
                        inventoryItem.LastStockUpdate = DateTime.Now;
                    }

                    break;

                case "rejected":
                    productRequest.ApprovedBy = request.UpdatedBy;
                    productRequest.ApprovalDate = DateTime.Now;
                    break;
            }

            if (!string.IsNullOrEmpty(request.Notes))
            {
                productRequest.Notes = string.IsNullOrEmpty(productRequest.Notes)
                    ? request.Notes
                    : $"{productRequest.Notes}\n{request.Notes}";
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductRequestExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return Ok(new
            {
                message = $"Request status updated from {oldStatus} to {request.Status}",
                newStatus = productRequest.RequestStatus
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductRequest(int id, UpdateProductRequestRequest request)
        {
            var productRequest = await _context.ProductRequests.FindAsync(id);
            if (productRequest == null)
            {
                return NotFound();
            }

            productRequest.RequestedQuantity = request.RequestedQuantity ?? productRequest.RequestedQuantity;
            productRequest.Notes = request.Notes ?? productRequest.Notes;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductRequestExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductRequest(int id)
        {
            var productRequest = await _context.ProductRequests.FindAsync(id);
            if (productRequest == null)
            {
                return NotFound();
            }

            if (productRequest.RequestStatus.ToLower() != "pending")
            {
                return BadRequest(new { message = "Only pending requests can be deleted" });
            }

            if (productRequest.TotalCost > 0)
            {
                await RestoreRevenueAsync(productRequest.Id, productRequest.TotalCost, "system", "ORDER_DELETED");
            }

            _context.ProductRequests.Remove(productRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductRequestExists(int id)
        {
            return _context.ProductRequests.Any(e => e.Id == id);
        }

        private async Task<bool> RestoreRevenueAsync(int productRequestId, decimal amount, string username,
            string transactionType)
        {
            try
            {
                var restoreRequest = new
                {
                    ProductRequestId = productRequestId,
                    Amount = amount,
                    TransactionType = transactionType,
                    CreatedBy = username,
                    Description = $"Revenue restored for {transactionType.ToLower().Replace('_', ' ')} by {username}"
                };

                var json = JsonSerializer.Serialize(restoreRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/revenue/restore", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Revenue restored: ${amount:F2} for request {productRequestId}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to restore revenue: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring revenue: {ex.Message}");
                return false;
            }
        }
    }
}