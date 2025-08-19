using System;

namespace LogisticsPro.UI.Models;

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Status { get; set; } = "Active";
    public DateTime RegisteredDate { get; set; } = DateTime.Now;
    
    // Default constructor
    public Client()
    {
    }
    
    // Parameterized constructor
    public Client(int id, string name, string contactPerson, string email, 
        string phone = null!, string address = null!, string city = null!, 
        string country = null!, string status = "Active")
    {
        Id = id;
        Name = name;
        ContactPerson = contactPerson;
        Email = email;
        Phone = phone;
        Address = address;
        City = city;
        Country = country;
        Status = status;
        RegisteredDate = DateTime.Now;
    }
}