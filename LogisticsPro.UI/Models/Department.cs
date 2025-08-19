using System.Collections.Generic;
using System.Linq;

namespace LogisticsPro.UI.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> AllowedRoles { get; set; } = new List<string>();
        public bool IsActive { get; set; } = true;
        
        // Collection of employees in this department
        public List<User> Employees { get; set; } = new List<User>();
        
        // Calculated property for employee count
        public int EmployeeCount => Employees?.Count ?? 0;
        
        // Constructor
        public Department(int id, string name, List<string> roles)
        {
            Id = id;
            Name = name;
            AllowedRoles = roles;
            Employees = new List<User>();
        }
        
        // Default constructor
        public Department() 
        {
            Employees = new List<User>();
        }
    }
}