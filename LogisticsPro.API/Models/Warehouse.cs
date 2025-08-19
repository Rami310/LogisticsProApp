
namespace LogisticsPro.API.Models
{
    public class Warehouse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Manager { get; set; } = null!;
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
}