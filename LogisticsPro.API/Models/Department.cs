using System;
using System.Collections.Generic;

namespace LogisticsPro.API.Models;

// Add this to your Models folder
// Add this to your Models folder
public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<string> AllowedRoles { get; set; } = new List<string>();
    public bool IsActive { get; set; } = true;
    
    // Constructor
    public Department(int id, string name, List<string> roles)
    {
        Id = id;
        Name = name;
        AllowedRoles = roles;
    }
    
    // Default constructor
    public Department() {}
}