using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticsPro.API.Data;
using LogisticsPro.API.Models;

namespace LogisticsPro.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing department operations in the logistics system.
    /// Provides CRUD operations and role management functionality for department entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly LogisticsDbContext _context;

        /// <summary>
        /// Initializes a new instance of the DepartmentsController class.
        /// </summary>
        /// <param name="context">The database context for logistics operations</param>
        public DepartmentsController(LogisticsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all active departments from the database, ordered by ID.
        /// </summary>
        /// <returns>A list of active departments</returns>
        /// <response code="200">Returns the list of active departments</response>
        /// <response code="500">If an internal server error occurs</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            try
            {
                var departments = await _context.Departments
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Id)
                    .ToListAsync();
                
                return Ok(departments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting departments: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves a specific department by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the department</param>
        /// <returns>The department with the specified ID</returns>
        /// <response code="200">Returns the requested department</response>
        /// <response code="404">If the department is not found</response>
        /// <response code="500">If an internal server error occurs</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                {
                    return NotFound($"Department with ID {id} not found");
                }

                return Ok(department);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting department {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves the allowed roles for a specific department.
        /// </summary>
        /// <param name="id">The unique identifier of the department</param>
        /// <returns>A list of allowed roles for the department</returns>
        /// <response code="200">Returns the list of allowed roles</response>
        /// <response code="404">If the department is not found</response>
        /// <response code="500">If an internal server error occurs</response>
        [HttpGet("{id}/roles")]
        public async Task<ActionResult<List<string>>> GetDepartmentRoles(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                {
                    return NotFound($"Department with ID {id} not found");
                }

                return Ok(department.AllowedRoles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting roles for department {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new department in the system.
        /// </summary>
        /// <param name="department">The department object to create</param>
        /// <returns>The newly created department with assigned ID</returns>
        /// <response code="201">Returns the newly created department</response>
        /// <response code="400">If the department data is invalid or name is missing</response>
        /// <response code="500">If an internal server error occurs</response>
        [HttpPost]
        public async Task<ActionResult<Department>> CreateDepartment(Department department)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(department.Name))
                {
                    return BadRequest("Department name is required");
                }

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, department);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating department: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing department's information.
        /// </summary>
        /// <param name="id">The ID of the department to update</param>
        /// <param name="department">The updated department data</param>
        /// <returns>No content on successful update</returns>
        /// <response code="204">Department updated successfully</response>
        /// <response code="400">If the ID in the URL doesn't match the department ID</response>
        /// <response code="404">If the department is not found</response>
        /// <response code="500">If an internal server error occurs</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, Department department)
        {
            if (id != department.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                _context.Entry(department).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(id))
                {
                    return NotFound($"Department with ID {id} not found");
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating department {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a department from the system.
        /// </summary>
        /// <param name="id">The ID of the department to delete</param>
        /// <returns>No content on successful deletion</returns>
        /// <response code="204">Department deleted successfully</response>
        /// <response code="404">If the department is not found</response>
        /// <response code="500">If an internal server error occurs</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                {
                    return NotFound($"Department with ID {id} not found");
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting department {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Checks if a department exists in the database.
        /// </summary>
        /// <param name="id">The ID of the department to check</param>
        /// <returns>True if the department exists, false otherwise</returns>
        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}