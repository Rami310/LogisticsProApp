using System;
using System.Collections.Generic;
using System.Linq;
using LogisticsPro.UI.Models;

namespace LogisticsPro.UI.Services
{
    public class ClientService : BaseService<Client>
    {
        // Singleton instance
        private static readonly ClientService _instance = new ClientService();

        // Public access to singleton
        public static ClientService Instance => _instance;

        // Mock data for development
        private static readonly List<Client> MockClients = new()
        {
            new Client
            {
                Id = 1, Name = "Acme Corporation", ContactPerson = "John Smith", Email = "jsmith@acme.com",
                Phone = "555-1234", Address = "123 Main St", City = "Metropolis", Country = "USA"
            },
            new Client
            {
                Id = 2, Name = "Globex Industries", ContactPerson = "Jane Doe", Email = "jane.doe@globex.com",
                Phone = "555-5678", Address = "456 Tech Blvd", City = "Silicon Valley", Country = "USA"
            },
            new Client
            {
                Id = 3, Name = "Wayne Enterprises", ContactPerson = "Bruce Wayne", Email = "bruce@wayne.com",
                Phone = "555-9012", Address = "1 Wayne Tower", City = "Gotham", Country = "USA"
            },
            new Client
            {
                Id = 4, Name = "Stark Industries", ContactPerson = "Tony Stark", Email = "tony@stark.com",
                Phone = "555-3456", Address = "10880 Malibu Point", City = "Malibu", Country = "USA"
            },
            new Client
            {
                Id = 5, Name = "LexCorp", ContactPerson = "Lex Luthor", Email = "lex@lexcorp.com", Phone = "555-7890",
                Address = "1000 Lexor Ave", City = "Metropolis", Country = "USA"
            }
        };

        // Private constructor for singleton
        private ClientService() : base(
            MockClients,
            client => client.Id,
            (client, id) => client.Id = id)
        {
        }

        // Get all clients (static method for backward compatibility)
        public static List<Client> GetAllClients()
        {
            return Instance.GetAll();
        }

        // Get client by ID (static method for backward compatibility)
        public static Client GetClientById(int id)
        {
            return Instance.GetById(id);
        }

        // Add client (static method for backward compatibility)
        public static void AddClient(Client client)
        {
            if (client == null) return;
            client.RegisteredDate = DateTime.Now;
            Instance.Add(client);
        }

        // Update client (static method for backward compatibility)
        public static void UpdateClient(Client client)
        {
            if (client == null) return;
            Instance.Update(client);
        }

        // Delete client (static method for backward compatibility)
        public static bool DeleteClient(int id)
        {
            return Instance.Delete(id);
        }

        // Advanced search with filters
        public static List<Client> SearchClients(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return Instance.GetAll();
            }

            var search = searchText.ToLower();

            // Filter by multiple fields
            return Instance.GetAll()
                .Where(c =>
                    c.Name?.ToLower().Contains(search) == true ||
                    c.ContactPerson?.ToLower().Contains(search) == true ||
                    c.Email?.ToLower().Contains(search) == true ||
                    c.City?.ToLower().Contains(search) == true ||
                    c.Country?.ToLower().Contains(search) == true)
                .ToList();
        }
    }
}