using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LogisticsPro.UI.Models;

namespace LogisticsPro.UI.Services
{
    public static class UserService
    {
        // Updated mock data with CORRECT role names matching database
        private static readonly List<User> MockEmployees = new()
        {
            new User
            {
                Id = 1, Username = "admin", Password = "1234", Role = "Administrator", Department = "Administration",
                DepartmentId = 1, Status = "Active", Name = "Admin", LastName = "User",
                Email = "admin@logisticspro.com", Phone = "555-1234"
            },
            new User
            {
                Id = 2, Username = "hrmanager",
                Password = "hr123", Role = "HR Manager",
                Department = "Human Resources",
                DepartmentId = 2, Status = "Active",
                Name = "Sarah", LastName = "Johnson",
                Email = "sarah@logisticspro.com",
                Phone = "555-4567"
            },
            new User
            {
                Id = 3, Username = "warehouse_mgr",
                Password = "wh123", Role = "Warehouse Manager",
                Department = "Warehouse", DepartmentId = 3,
                Status = "Active", Name = "John", LastName = "Manager",
                Email = "john@logisticspro.com", Phone = "555-2345" 
            },
            // Changed from "Employee" to "Logistics Employee"
            new User
            {
                Id = 4, Username = "employee", Password = "abcd", Role = "Logistics Employee", Department = "Logistics",
                DepartmentId = 4, Status = "Active", Name = "Jane", LastName = "Smith", Email = "jane@logisticspro.com",
                Phone = "555-3456"
            },
            // Changed from "Employee" to "Warehouse Employee" 
            new User
            {
                Id = 5, Username = "warehouse1", Password = "wh123", Role = "Warehouse Employee", Department = "Warehouse",
                DepartmentId = 3, Status = "Active", Name = "Mike", LastName = "Warehouse",
                Email = "mike@logisticspro.com", Phone = "555-5678"
            },
            // Additional test accounts matching the database
            new User
            {
                Id = 6, Username = "wh_emp1", Password = "emp123", Role = "Warehouse Employee", Department = "Warehouse",
                DepartmentId = 3, Status = "Active", Name = "Emma", LastName = "Davis",
                Email = "emma.davis@logisticspro.com", Phone = "555-6789"
            },
            new User
            {
                Id = 7, Username = "logistics_mgr", Password = "log123", Role = "Logistics Manager", Department = "Logistics",
                DepartmentId = 4, Status = "Active", Name = "David", LastName = "Brown",
                Email = "david.brown@logisticspro.com", Phone = "555-7890"
            },
            new User
            {
                Id = 8, Username = "log_emp1", Password = "emp123", Role = "Logistics Employee", Department = "Logistics",
                DepartmentId = 4, Status = "Active", Name = "Robert", LastName = "Martinez",
                Email = "robert.martinez@logisticspro.com", Phone = "555-8901"
            }
        };

        /// <summary>
        /// API-first user authentication with intelligent fallback
        /// </summary>
        public static async Task<User?> ValidateUserAsync(string username, string password)
        {
            try
            {
                Console.WriteLine($"🌐 Calling API: {ApiConfiguration.BaseUrl}Users/login");

                var loginData = new { Username = username, Password = password };
                var json = JsonSerializer.Serialize(loginData, ApiConfiguration.JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ApiConfiguration.HttpClient.PostAsync("Users/login", content);

                Console.WriteLine($"📡 Response Status: {response.StatusCode}");
                Console.WriteLine($"📍 Full URL called: {ApiConfiguration.HttpClient.BaseAddress}Users/login");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✅ Login successful: {responseContent}");
                    var user = JsonSerializer.Deserialize<User>(responseContent, ApiConfiguration.JsonOptions);
                    return user;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Login failed: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Login exception: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get all employees from API with intelligent fallback
        /// </summary>
        public static async Task<List<User>> GetAllEmployeesAsync()
        {
            try
            {
                Console.WriteLine($"🌐 Calling API: {ApiConfiguration.BaseUrl}Users");
                
                var response = await ApiConfiguration.HttpClient.GetAsync("Users");

                Console.WriteLine($"📡 Response Status: {response.StatusCode}");
                Console.WriteLine($"📍 Full URL called: {ApiConfiguration.HttpClient.BaseAddress}Users");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✅ Users loaded successfully, content length: {content.Length}");
                    var users = JsonSerializer.Deserialize<List<User>>(content, ApiConfiguration.JsonOptions);
                    return users ?? new List<User>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Users loading failed: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Users loading exception: {ex.Message}");
            }

            Console.WriteLine("🔄 Falling back to mock data");
            return GetMockEmployees();
        }

        /// <summary>
        /// Add user via API with fallback
        /// </summary>
        public static async Task<User?> AddUserAsync(User user)
        {
            Console.WriteLine($"➕ Adding user: {user.Username}");

            try
            {
                var apiAvailable = await ApiConfiguration.IsApiAvailableAsync();

                if (apiAvailable)
                {
                    var createRequest = new
                    {
                        Username = user.Username,
                        Password = user.Password,
                        Role = user.Role,
                        Name = user.Name,
                        LastName = user.LastName,
                        Department = user.Department,
                        DepartmentId = user.DepartmentId,
                        Email = user.Email,
                        Phone = user.Phone
                    };

                    var json = JsonSerializer.Serialize(createRequest, ApiConfiguration.JsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await ApiConfiguration.HttpClient.PostAsync("Users", content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Get the created user back from API with correct ID
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var createdUser =
                            JsonSerializer.Deserialize<User>(responseContent, ApiConfiguration.JsonOptions);

                        Console.WriteLine(
                            $"✅ User {user.Username} added successfully via API with ID: {createdUser?.Id}");
                        return createdUser; // Return the user with database ID
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ API add user failed: {response.StatusCode}");
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error details: {errorContent}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏱️ API add user timeout, using mock addition");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API add user error: {ex.Message}");
            }

            // Fall back to mock addition
            Console.WriteLine("🔄 Adding user to mock data");
            AddUser(user); // This sets the ID in mock data
            return user; // Return user with mock ID
        }
        

        // ========================================
        // MOCK DATA METHODS (Fallback)
        // ========================================

        /// <summary>
        /// Fallback authentication using mock data
        /// </summary>
        public static User? ValidateUserMock(string username, string password)
        {

            Console.WriteLine($"🔍 Mock authentication for: {username}");

            var user = MockEmployees.FirstOrDefault(u =>
                u.Username?.ToLower() == username.ToLower() &&
                u.Password == password);

            if (user != null)
            {
                Console.WriteLine($"✅ Mock login successful: {user.Username} -> {user.Role}");
            }
            else
            {
                Console.WriteLine("❌ Mock login failed: Invalid credentials");
            }

            return user;
        }

        /// <summary>
        /// Get mock employees for fallback
        /// </summary>
        public static List<User> GetMockEmployees()
        {
            return MockEmployees.ToList();
        }

        /// <summary>
        /// Get user by username from mock data
        /// </summary>
        public static User? GetUserByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            return MockEmployees.FirstOrDefault(u =>
                u.Username?.ToLower() == username.ToLower());
        }

        /// <summary>
        /// Validate user credentials against mock data
        /// </summary>
        public static bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            return MockEmployees.Any(user =>
                user.Username?.ToLower() == username.ToLower() &&
                user.Password == password);
        }

        /// <summary>
        /// Add user to mock data (fallback)
        /// </summary>
        public static void AddUser(User? user)
        {
            if (user == null)
                return;

            // Generate a new ID
            var newId = MockEmployees.Count > 0 ? MockEmployees.Max(u => u.Id) + 1 : 1;
            user.Id = newId;

            MockEmployees.Add(user);
            Console.WriteLine($"✅ User {user.Username} added to mock data");
        }

        // ========================================
        // SYNCHRONOUS WRAPPER METHODS
        // ========================================

        /// <summary>
        /// Synchronous wrapper for ValidateUserAsync
        /// </summary>
        public static User? ValidateUserSync(string username, string password)
        {
            try
            {
                var task = ValidateUserAsync(username, password);
                task.Wait(TimeSpan.FromSeconds(10));
                return task.Result;
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                Console.WriteLine("⏱️ Sync login timeout, using mock data");
                return ValidateUserMock(username, password);
            }
            catch
            {
                Console.WriteLine("❌ Sync login failed, using mock data");
                return ValidateUserMock(username, password);
            }
        }

        public static async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                Console.WriteLine($"🗑️ Attempting to delete user ID: {userId}");
                Console.WriteLine($"🌐 Calling API: {ApiConfiguration.BaseUrl}Users/{userId}");
                
                var response = await ApiConfiguration.HttpClient.DeleteAsync($"Users/{userId}");

                Console.WriteLine($"📡 Response Status: {response.StatusCode}");
                Console.WriteLine($"📍 Full URL called: {ApiConfiguration.HttpClient.BaseAddress}Users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ User {userId} deleted from database");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Delete failed: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Delete API error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                Console.WriteLine($"🔄 Attempting to update user: {user.Username}");
                Console.WriteLine($"🌐 Calling API: {ApiConfiguration.BaseUrl}Users/{user.Id}");

                // Create update request
                var updateRequest = new
                {
                    password = user.Password,
                    role = user.Role,
                    name = user.Name,
                    lastName = user.LastName,
                    department = user.Department,
                    departmentId = user.DepartmentId,
                    email = user.Email,
                    phone = user.Phone,
                    status = user.Status
                };

                var json = JsonSerializer.Serialize(updateRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await ApiConfiguration.HttpClient.PutAsync($"Users/{user.Id}", content);

                Console.WriteLine($"📡 Response Status: {response.StatusCode}");
                Console.WriteLine($"📍 Full URL called: {ApiConfiguration.HttpClient.BaseAddress}Users/{user.Id}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ User {user.Username} updated in database");

                    // local mock data for consistency
                    var existingUser = MockEmployees.FirstOrDefault(u => u.Id == user.Id);
                    if (existingUser != null)
                    {
                        existingUser.Name = user.Name;
                        existingUser.LastName = user.LastName;
                        existingUser.Email = user.Email;
                        existingUser.Phone = user.Phone;
                        existingUser.Department = user.Department;
                        existingUser.DepartmentId = user.DepartmentId;
                        existingUser.Role = user.Role;
                        existingUser.Status = user.Status;
                        Console.WriteLine($"✅ Local mock data also updated");
                    }

                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Update failed: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Update API error: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// DTO for API responses
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Status { get; set; } = "Active";
    }
    
    
}