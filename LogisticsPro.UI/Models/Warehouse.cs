public class Warehouse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string Address { get; set; }
    public string Manager { get; set; }
    public string Status { get; set; } = "Active";
    
    // Default constructor
    public Warehouse()
    {
    }
    
    // Parameterized constructor
    public Warehouse(int id, string name, string location, string address, 
        string manager, string status = "Active")
    {
        Id = id;
        Name = name;
        Location = location;
        Address = address;
        Manager = manager;
        Status = status;
    }
}