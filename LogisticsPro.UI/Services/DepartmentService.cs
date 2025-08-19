using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using LogisticsPro.UI.Models;
using LogisticsPro.UI.Services;

namespace LogisticsPro.UI.Services;

public static class DepartmentService
{
    private static readonly string _apiBaseUrl = ApiConfiguration.BaseUrl;
    
    public static readonly List<string> SystemRoles = new()
    {
        "Administrator",
        "HR Manager",
        "Warehouse Manager",
        "Warehouse Employee",
        "Logistics Manager",
        "Logistics Employee"
    };
    
    public static List<Department> GetAllDepartments()
    {
        try
        {
            Console.WriteLine("DepartmentService.GetAllDepartments() called");
            
            Console.WriteLine("Calling real API for departments...");
            return Task.Run(async () => await GetAllDepartmentsAsync()).Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllDepartments: {ex.Message}");
            Console.WriteLine("Falling back to mock data");
            return GetMockDepartments();
        }
    }
    
    public static async Task<List<Department>> GetAllDepartmentsAsync()
    {
        try
        {
            Console.WriteLine("Attempting to call departments API...");
            Console.WriteLine($"API Base URL: {_apiBaseUrl}");
            
            var apiDepartments = await GetDepartmentsFromApi();
            
            if (apiDepartments?.Any() == true)
            {
                Console.WriteLine($"API returned {apiDepartments.Count} departments");
                return TransformApiDepartmentsForUI(apiDepartments);
            }
            else
            {
                Console.WriteLine("API returned no departments, using mock data");
                return GetMockDepartments();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call failed: {ex.Message}");
            Console.WriteLine($"Using mock data as fallback");
            return GetMockDepartments();
        }
    }
    
    private static async Task<List<Department>> GetDepartmentsFromApi()
    {
        try
        {
            Console.WriteLine("Making HTTP request to departments endpoint");
            
            var url = $"{_apiBaseUrl}departments";
            Console.WriteLine($"Full URL: {url}");
            
            var response = await ApiConfiguration.HttpClient.GetAsync("departments");
            Console.WriteLine($"Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content length: {jsonString.Length} characters");
                
                var departments = JsonSerializer.Deserialize<List<Department>>(jsonString, ApiConfiguration.JsonOptions);
                
                Console.WriteLine($"Deserialized {departments?.Count ?? 0} departments");
                return departments ?? new List<Department>();
            }
            else
            {
                Console.WriteLine($"API call failed with status: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error content: {errorContent}");
                return new List<Department>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetDepartmentsFromApi: {ex.Message}");
            throw;
        }
    }
    
    private static List<Department> TransformApiDepartmentsForUI(List<Department> apiDepartments)
    {
        Console.WriteLine($"Transforming {apiDepartments.Count} API departments for UI");
        
        var uiDepartments = new List<Department>();
        int idCounter = 1;
        
        foreach (var apiDept in apiDepartments)
        {
            Console.WriteLine($"Processing department: {apiDept.Name} with {apiDept.AllowedRoles?.Count ?? 0} roles");
            
            if (apiDept.AllowedRoles?.Count == 1)
            {
                uiDepartments.Add(new Department(idCounter++, apiDept.Name, apiDept.AllowedRoles));
                Console.WriteLine($"   Kept as-is: {apiDept.Name}");
            }
            else if (apiDept.AllowedRoles?.Count > 1)
            {
                foreach (var role in apiDept.AllowedRoles)
                {
                    var departmentName = GetDisplayNameForRole(apiDept.Name, role);
                    uiDepartments.Add(new Department(idCounter++, departmentName, new List<string> { role }));
                    Console.WriteLine($"   Split: {apiDept.Name} → {departmentName} ({role})");
                }
            }
        }
        
        Console.WriteLine($"Transformation complete: {uiDepartments.Count} UI departments");
        return uiDepartments;
    }
    
    private static string GetDisplayNameForRole(string baseDepartmentName, string role)
    {
        return role switch
        {
            "Warehouse Manager" => "Warehouse Management",
            "Warehouse Employee" => "Warehouse Operations", 
            "Logistics Manager" => "Logistics Management",
            "Logistics Employee" => "Logistics Operations",
            _ => baseDepartmentName
        };
    }
    
    private static List<Department> GetMockDepartments()
    {
        Console.WriteLine("Using mock departments");
        return new List<Department>
        {
            new Department(1, "Administration", new List<string> { "Administrator" }),
            new Department(2, "Human Resources", new List<string> { "HR Manager" }),
            new Department(3, "Warehouse Management", new List<string> { "Warehouse Manager" }),
            new Department(4, "Warehouse Operations", new List<string> { "Warehouse Employee" }),
            new Department(5, "Logistics Management", new List<string> { "Logistics Manager" }),
            new Department(6, "Logistics Operations", new List<string> { "Logistics Employee" })
        };
    }
    
    public static Department GetDepartmentById(int id)
    {
        var departments = GetAllDepartments();
        return departments.FirstOrDefault(d => d.Id == id);
    }
    
    public static List<string> GetRolesForDepartment(int departmentId)
    {
        var department = GetDepartmentById(departmentId);
        return department?.AllowedRoles ?? new List<string>();
    }
}