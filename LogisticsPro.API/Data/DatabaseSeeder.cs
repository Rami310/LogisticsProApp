using LogisticsPro.API.Data;
using LogisticsPro.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LogisticsPro.API.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedData(LogisticsDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Departments.Any())
            {
                return; // Data already seeded
            }

            // Seed in proper order
            SeedDepartments(context);
            SeedUsers(context);
            SeedWarehouses(context);
            SeedProducts(context);
            SeedInventoryItems(context);
            SeedProductRequests(context);
            SeedClients(context);

            Console.WriteLine("Database seeded successfully with new roles!");
        }

        private static void SeedDepartments(LogisticsDbContext context)
        {
            if (context.Departments.Any()) return;

            var departments = new List<Department>
            {
                new Department
                {
                    Id = 1,
                    Name = "Administration",
                    AllowedRoles = new List<string> { "Administrator" },
                    IsActive = true
                },
                new Department
                {
                    Id = 2,
                    Name = "Human Resources",
                    AllowedRoles = new List<string> { "HR Manager" },
                    IsActive = true
                },
                new Department
                {
                    Id = 3,
                    Name = "Warehouse",
                    AllowedRoles = new List<string> { "Warehouse Manager", "Warehouse Employee" },
                    IsActive = true
                },
                new Department
                {
                    Id = 4,
                    Name = "Logistics",
                    AllowedRoles = new List<string> { "Logistics Manager", "Logistics Employee" },
                    IsActive = true
                }
            };

            context.Departments.AddRange(departments);
            context.SaveChanges();
            Console.WriteLine($"✅ Seeded {departments.Count} departments");
        }

        private static void SeedUsers(LogisticsDbContext context)
        {
            if (context.Users.Any()) return;

            var users = new List<User>
            {
                // Administration
                new User
                {
                    Username = "admin", Password = "1234", Role = "Administrator", Name = "Admin", LastName = "User",
                    Department = "Administration", DepartmentId = 1, Email = "admin@logisticspro.com",
                    Phone = "555-1234", Status = "Active"
                },

                // Human Resources
                new User
                {
                    Username = "hrmanager", Password = "hr123", Role = "HR Manager", Name = "Sarah",
                    LastName = "Johnson", Department = "Human Resources", DepartmentId = 2,
                    Email = "sarah@logisticspro.com", Phone = "555-4567", Status = "Active"
                },

                // Warehouse Department
                new User
                {
                    Username = "warehouse_mgr", Password = "wh123", Role = "Warehouse Manager", Name = "John",
                    LastName = "Manager", Department = "Warehouse", DepartmentId = 3, Email = "john@logisticspro.com",
                    Phone = "555-2345", Status = "Active"
                },
                new User
                {
                    Username = "wh_emp1", Password = "emp123", Role = "Warehouse Employee", Name = "Mike",
                    LastName = "Warehouse", Department = "Warehouse", DepartmentId = 3, Email = "mike@logisticspro.com",
                    Phone = "555-5678", Status = "Active"
                },
                new User
                {
                    Username = "wh_emp2", Password = "emp123", Role = "Warehouse Employee", Name = "Emma",
                    LastName = "Davis", Department = "Warehouse", DepartmentId = 3,
                    Email = "emma.davis@logisticspro.com", Phone = "555-6789", Status = "Active"
                },

                // Logistics Department  
                new User
                {
                    Username = "logistics_mgr", Password = "log123", Role = "Logistics Manager", Name = "David",
                    LastName = "Brown", Department = "Logistics", DepartmentId = 4,
                    Email = "david.brown@logisticspro.com", Phone = "555-7890", Status = "Active"
                },
                new User
                {
                    Username = "log_emp1", Password = "emp123", Role = "Logistics Employee", Name = "Jane",
                    LastName = "Smith", Department = "Logistics", DepartmentId = 4, Email = "jane@logisticspro.com",
                    Phone = "555-3456", Status = "Active"
                },
                new User
                {
                    Username = "log_emp2", Password = "emp123", Role = "Logistics Employee", Name = "Robert",
                    LastName = "Martinez", Department = "Logistics", DepartmentId = 4,
                    Email = "robert.martinez@logisticspro.com", Phone = "555-8901", Status = "Active"
                },
                new User
                {
                    Username = "log_emp3", Password = "emp123", Role = "Logistics Employee", Name = "Maria",
                    LastName = "Garcia", Department = "Logistics", DepartmentId = 4,
                    Email = "maria.garcia@logisticspro.com", Phone = "555-2468", Status = "Active"
                },
                new User
                {
                    Username = "log_emp4", Password = "emp123", Role = "Logistics Employee", Name = "Alex",
                    LastName = "Thompson", Department = "Logistics", DepartmentId = 4,
                    Email = "alex.thompson@logisticspro.com", Phone = "555-3579", Status = "Active"
                },
                new User
                {
                    Username = "log_emp5", Password = "emp123", Role = "Logistics Employee", Name = "Sarah",
                    LastName = "Wilson", Department = "Logistics", DepartmentId = 4,
                    Email = "sarah.wilson@logisticspro.com", Phone = "555-4680", Status = "Active"
                },
                new User
                {
                    Username = "log_emp6", Password = "emp123", Role = "Logistics Employee", Name = "Chris",
                    LastName = "Lee", Department = "Logistics", DepartmentId = 4, Email = "chris.lee@logisticspro.com",
                    Phone = "555-5791", Status = "Active"
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
            Console.WriteLine($"✅ Seeded {users.Count} users");
        }

        private static void SeedWarehouses(LogisticsDbContext context)
        {
            if (context.Warehouses.Any()) return;
            
            var warehouses = new List<Warehouse>
            {
                new Warehouse { Name = "Main Warehouse", Location = "Northern District", Address = "123 Logistics Ave", Manager = "John Manager", Status = "Active" },
                new Warehouse { Name = "Southern Depot", Location = "Southern District", Address = "456 Supply Chain Blvd", Manager = "Mike Warehouse", Status = "Active" }
            };

            context.Warehouses.AddRange(warehouses);
            context.SaveChanges();
            Console.WriteLine($"✅ Seeded {warehouses.Count} warehouses");
        }

        private static void SeedProducts(LogisticsDbContext context)
        {
            if (context.Products.Any()) return;
            
            var products = new List<Product>
            {
                new Product { Name = "Standard Cardboard Box", Description = "Standard size cardboard box for general shipping", SKU = "BOX-STD-001", Category = "Packaging", UnitPrice = 2.50M, UnitOfMeasure = "Each", Supplier = "PackWell Inc.", Status = "Active", CreatedDate = DateTime.Now },
                new Product { Name = "Bubble Wrap (Small)", Description = "Small bubble wrap roll, 50ft", SKU = "WRAP-BUB-S", Category = "Packaging", UnitPrice = 15.75M, UnitOfMeasure = "Roll", Supplier = "SafePack", Status = "Active", CreatedDate = DateTime.Now },
                new Product { Name = "Pallet Jack", Description = "Standard manual pallet jack, 5000 lb capacity", SKU = "EQUIP-PAL-001", Category = "Equipment", UnitPrice = 299.99M, UnitOfMeasure = "Each", Supplier = "Heavy Lifters Co.", Status = "Active", CreatedDate = DateTime.Now },
                new Product { Name = "Shipping Labels", Description = "Pack of 100 adhesive shipping labels", SKU = "LAB-SHP-100", Category = "Stationery", UnitPrice = 9.99M, UnitOfMeasure = "Pack", Supplier = "OfficeMart", Status = "Active", CreatedDate = DateTime.Now },
                new Product { Name = "Forklift Battery", Description = "Replacement battery for standard warehouse forklift", SKU = "EQUIP-FORK-BAT", Category = "Equipment", UnitPrice = 1200.00M, UnitOfMeasure = "Each", Supplier = "PowerLift", Status = "Active", CreatedDate = DateTime.Now }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
            Console.WriteLine($"✅ Seeded {products.Count} products");
        }

        private static void SeedInventoryItems(LogisticsDbContext context)
        {
            if (context.InventoryItems.Any()) return;
            
            var inventoryItems = new List<InventoryItem>
            {
                new InventoryItem { ProductId = 1, WarehouseId = 1, Location = "A1-01", QuantityInStock = 250, MinimumStockLevel = 50, MaximumStockLevel = 500, LastStockUpdate = DateTime.Now },
                new InventoryItem { ProductId = 2, WarehouseId = 1, Location = "A2-05", QuantityInStock = 30, MinimumStockLevel = 10, MaximumStockLevel = 50, LastStockUpdate = DateTime.Now },
                new InventoryItem { ProductId = 3, WarehouseId = 1, Location = "B3-12", QuantityInStock = 5, MinimumStockLevel = 2, MaximumStockLevel = 10, LastStockUpdate = DateTime.Now },
                new InventoryItem { ProductId = 4, WarehouseId = 1, Location = "C1-08", QuantityInStock = 120, MinimumStockLevel = 40, MaximumStockLevel = 200, LastStockUpdate = DateTime.Now },
                new InventoryItem { ProductId = 5, WarehouseId = 1, Location = "B5-20", QuantityInStock = 8, MinimumStockLevel = 3, MaximumStockLevel = 15, LastStockUpdate = DateTime.Now }
            };

            context.InventoryItems.AddRange(inventoryItems);
            context.SaveChanges();
            Console.WriteLine($"✅ Seeded {inventoryItems.Count} inventory items");
        }

        private static void SeedProductRequests(LogisticsDbContext context)
        {
            if (context.ProductRequests.Any()) return;
            
            var productRequests = new List<ProductRequest>
            {
                new ProductRequest { ProductId = 1, RequestedQuantity = 100, RequestedBy = "warehouse_mgr", RequestStatus = "Pending", RequestDate = DateTime.Now, Notes = "Regular restock" },
                new ProductRequest { ProductId = 3, RequestedQuantity = 2, RequestedBy = "warehouse_mgr", RequestStatus = "Approved", RequestDate = DateTime.Now.AddDays(-3), ApprovedBy = "admin", ApprovalDate = DateTime.Now.AddDays(-2), Notes = "Urgent need" },
                new ProductRequest { ProductId = 2, RequestedQuantity = 15, RequestedBy = "warehouse_mgr", RequestStatus = "Received", RequestDate = DateTime.Now.AddDays(-7), ApprovedBy = "admin", ApprovalDate = DateTime.Now.AddDays(-5), ReceivedBy = "wh_emp1", ReceivedDate = DateTime.Now.AddDays(-1), Notes = "Monthly restock" }
            };

            context.ProductRequests.AddRange(productRequests);
            context.SaveChanges();
            Console.WriteLine($"✅ Seeded {productRequests.Count} product requests");
        }

        private static void SeedClients(LogisticsDbContext context)
        {
            if (context.Clients.Any()) return;
            
            var clients = new List<Client>
            {
                new Client { Name = "Acme Corporation", ContactPerson = "John Smith", Email = "jsmith@acme.com", Phone = "555-1234", Address = "123 Main St", City = "Metropolis", Country = "USA", Status = "Active", RegisteredDate = DateTime.Now.AddMonths(-6) },
                new Client { Name = "Globex Industries", ContactPerson = "Jane Doe", Email = "jane.doe@globex.com", Phone = "555-5678", Address = "456 Tech Blvd", City = "Silicon Valley", Country = "USA", Status = "Active", RegisteredDate = DateTime.Now.AddMonths(-4) },
                new Client { Name = "Wayne Enterprises", ContactPerson = "Bruce Wayne", Email = "bruce@wayne.com", Phone = "555-9012", Address = "1 Wayne Tower", City = "Gotham", Country = "USA", Status = "Active", RegisteredDate = DateTime.Now.AddMonths(-3) },
                new Client { Name = "Stark Industries", ContactPerson = "Tony Stark", Email = "tony@stark.com", Phone = "555-3456", Address = "10880 Malibu Point", City = "Malibu", Country = "USA", Status = "Active", RegisteredDate = DateTime.Now.AddMonths(-2) },
                new Client { Name = "LexCorp", ContactPerson = "Lex Luthor", Email = "lex@lexcorp.com", Phone = "555-7890", Address = "1000 Lexor Ave", City = "Metropolis", Country = "USA", Status = "Active", RegisteredDate = DateTime.Now.AddMonths(-1) }
            };
            
            context.Clients.AddRange(clients);
            context.SaveChanges();
            Console.WriteLine($"✅ Seeded {clients.Count} clients");
        }
        
         public static async Task SeedRevenueDataAsync(LogisticsDbContext context)
        {
            try
            {
                // Check if revenue data already exists
                var existingRevenue = await context.CompanyRevenue.FirstOrDefaultAsync();
                
                if (existingRevenue == null)
                {
                    Console.WriteLine("💰 Creating initial revenue record...");
                    
                    var initialRevenue = new CompanyRevenue
                    {
                        CurrentRevenue = 1500000.00m,
                        AvailableBudget = 1499795.25m,  // Slightly less to show some spending
                        TotalSpent = 204.75m,
                        UpdatedBy = "system",
                        UpdateReason = "Initial company budget setup",
                        LastUpdated = DateTime.Now
                    };

                    context.CompanyRevenue.Add(initialRevenue);
                    await context.SaveChangesAsync();
                    
                    Console.WriteLine("✅ Initial revenue data seeded successfully");
                    Console.WriteLine($"💰 Budget: ${initialRevenue.CurrentRevenue:N0}, Available: ${initialRevenue.AvailableBudget:N0}");
                }
                else
                {
                    Console.WriteLine($"💰 Revenue data already exists - Available Budget: ${existingRevenue.AvailableBudget:N0}");
                }
                
                // Also seed some sample transactions if needed
                var transactionCount = await context.RevenueTransactions.CountAsync();
                if (transactionCount == 0)
                {
                    Console.WriteLine("💳 Creating sample revenue transactions...");
                    
                    var sampleTransactions = new List<RevenueTransaction>
                    {
                        new RevenueTransaction 
                        { 
                            TransactionType = "ORDER_PLACED", 
                            Amount = 99.90m, 
                            CreatedBy = "warehouse_mgr", 
                            CreatedDate = DateTime.Now.AddDays(-2),
                            Description = "Office supplies order",
                            BalanceAfter = 1499895.35m
                        },
                        new RevenueTransaction 
                        { 
                            TransactionType = "ORDER_PLACED", 
                            Amount = 104.85m, 
                            CreatedBy = "warehouse_mgr", 
                            CreatedDate = DateTime.Now.AddDays(-1),
                            Description = "Packaging materials order",
                            BalanceAfter = 1499795.25m
                        }
                    };
                    
                    context.RevenueTransactions.AddRange(sampleTransactions);
                    await context.SaveChangesAsync();
                    
                    Console.WriteLine($"✅ Seeded {sampleTransactions.Count} sample revenue transactions");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to seed revenue data: {ex.Message}");
                Console.WriteLine($"🔍 Exception details: {ex.InnerException?.Message}");
            }
        }
    }
}