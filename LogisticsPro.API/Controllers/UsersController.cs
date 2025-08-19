using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticsPro.API.Data;
using LogisticsPro.API.Models;

namespace LogisticsPro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly LogisticsDbContext _context;

        public UsersController(LogisticsDbContext context)
        {
            _context = context;
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Return user info without password
            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                role = user.Role,
                name = user.Name,
                lastName = user.LastName,
                department = user.Department,
                email = user.Email,
                status = user.Status
            });
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Users.Select(u => new
            {
                id = u.Id,
                username = u.Username,
                role = u.Role,
                name = u.Name,
                lastName = u.LastName,
                department = u.Department,
                email = u.Email,
                phone = u.Phone,
                status = u.Status
            }).ToListAsync();

            return Ok(users);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Return user without password
            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                role = user.Role,
                name = user.Name,
                lastName = user.LastName,
                department = user.Department,
                email = user.Email,
                phone = user.Phone,
                status = user.Status
            });
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<object>> CreateUser(CreateUserRequest request)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            var user = new User
            {
                Username = request.Username,
                Password = request.Password, // In production, hash this!
                Role = request.Role,
                Name = request.Name,
                LastName = request.LastName,
                Department = request.Department,
                DepartmentId = request.DepartmentId,
                Email = request.Email,
                Phone = request.Phone,
                Status = "Active"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                id = user.Id,
                username = user.Username,
                role = user.Role,
                name = user.Name,
                lastName = user.LastName,
                department = user.Department,
                email = user.Email,
                phone = user.Phone,
                status = user.Status
            });
        }

        
        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Role = request.Role ?? user.Role;
            user.Name = request.Name ?? user.Name;
            user.LastName = request.LastName ?? user.LastName;
            user.Department = request.Department ?? user.Department;
            user.DepartmentId = request.DepartmentId ?? user.DepartmentId;
            user.Email = request.Email ?? user.Email;
            user.Phone = request.Phone ?? user.Phone;
            user.Status = request.Status ?? user.Status;

            // Only update password if provided
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.Password = request.Password; // In production, hash this!
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }

    // Request DTOs
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        public int DepartmentId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class UpdateUserRequest
    {
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        public int? DepartmentId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
    }
}