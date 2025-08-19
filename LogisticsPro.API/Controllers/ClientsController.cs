using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticsPro.API.Data;
using LogisticsPro.API.Models;

namespace LogisticsPro.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing client operations in the logistics system.
    /// Provides CRUD operations and search functionality for client entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly LogisticsDbContext _context;

        /// <summary>
        /// Initializes a new instance of the ClientsController class.
        /// </summary>
        /// <param name="context">The database context for logistics operations</param>
        public ClientsController(LogisticsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all clients from the database.
        /// </summary>
        /// <returns>A list of all clients</returns>
        /// <response code="200">Returns the list of clients</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            return await _context.Clients.ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific client by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the client</param>
        /// <returns>The client with the specified ID</returns>
        /// <response code="200">Returns the requested client</response>
        /// <response code="404">If the client is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        /// <summary>
        /// Searches for clients based on a query string that matches various client properties.
        /// </summary>
        /// <param name="query">The search term to match against client name, contact person, email, city, or country</param>
        /// <returns>A list of clients matching the search criteria</returns>
        /// <response code="200">Returns the list of matching clients</response>
        /// <remarks>
        /// The search is case-insensitive and searches across multiple fields:
        /// - Client Name
        /// - Contact Person
        /// - Email Address
        /// - City
        /// - Country
        /// 
        /// If no query is provided, returns all clients.
        /// </remarks>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Client>>> SearchClients([FromQuery] string query)
        {
            // If no search query provided, return all clients
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetClients();
            }

            // Convert search term to lowercase for case-insensitive comparison
            var searchTerm = query.ToLower();
            
            // Search across multiple client properties
            var clients = await _context.Clients
                .Where(c => c.Name.ToLower().Contains(searchTerm) ||
                           c.ContactPerson.ToLower().Contains(searchTerm) ||
                           c.Email.ToLower().Contains(searchTerm) ||
                           (c.City != null && c.City.ToLower().Contains(searchTerm)) ||
                           (c.Country != null && c.Country.ToLower().Contains(searchTerm)))
                .ToListAsync();

            return Ok(clients);
        }

        /// <summary>
        /// Creates a new client in the system.
        /// </summary>
        /// <param name="client">The client object to create</param>
        /// <returns>The newly created client with assigned ID</returns>
        /// <response code="201">Returns the newly created client</response>
        /// <response code="400">If the client data is invalid</response>
        [HttpPost]
        public async Task<ActionResult<Client>> CreateClient(Client client)
        {
            // Set the registration date to current timestamp
            client.RegisteredDate = DateTime.Now;
            
            // Add client to context and save changes
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // Return 201 Created with location header pointing to the new resource
            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
        }

        /// <summary>
        /// Updates an existing client's information.
        /// </summary>
        /// <param name="id">The ID of the client to update</param>
        /// <param name="client">The updated client data</param>
        /// <returns>No content on successful update</returns>
        /// <response code="204">Client updated successfully</response>
        /// <response code="400">If the ID in the URL doesn't match the client ID</response>
        /// <response code="404">If the client is not found</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, Client client)
        {
            // Ensure the ID in the URL matches the client object ID
            if (id != client.Id)
            {
                return BadRequest();
            }

            // Mark the entity as modified
            _context.Entry(client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency conflicts - check if client still exists
                if (!ClientExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a client from the system.
        /// </summary>
        /// <param name="id">The ID of the client to delete</param>
        /// <returns>No content on successful deletion</returns>
        /// <response code="204">Client deleted successfully</response>
        /// <response code="404">If the client is not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            // Find the client to delete
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            // Remove client and save changes
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Checks if a client exists in the database.
        /// </summary>
        /// <param name="id">The ID of the client to check</param>
        /// <returns>True if the client exists, false otherwise</returns>
        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}