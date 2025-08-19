using System;
using System.Collections.Generic;
using System.Linq;
using LogisticsPro.UI.Models;

namespace LogisticsPro.UI.Services;

// Warehouse Service
public static class WarehouseService
{
    // Mock data for development
    private static readonly List<Warehouse> MockWarehouses = new()
    {
        new Warehouse { Id = 1, Name = "Main Warehouse", Location = "Northern District", Address = "123 Logistics Ave", Manager = "manager" },
        new Warehouse { Id = 2, Name = "Southern Depot", Location = "Southern District", Address = "456 Supply Chain Blvd", Manager = "employee" }
    };

    public static List<Warehouse> GetAllWarehouses()
    {
        return MockWarehouses;
    }

    public static Warehouse GetWarehouseById(int id)
    {
        return MockWarehouses.FirstOrDefault(w => w.Id == id);
    }

    public static void AddWarehouse(Warehouse warehouse)
    {
        // Generate a new ID
        var newId = MockWarehouses.Count > 0 ? MockWarehouses.Max(w => w.Id) + 1 : 1;
        warehouse.Id = newId;
            
        MockWarehouses.Add(warehouse);
    }

    public static void UpdateWarehouse(Warehouse warehouse)
    {
        var existingWarehouse = MockWarehouses.FirstOrDefault(w => w.Id == warehouse.Id);
        if (existingWarehouse != null)
        {
            existingWarehouse.Name = warehouse.Name;
            existingWarehouse.Location = warehouse.Location;
            existingWarehouse.Address = warehouse.Address;
            existingWarehouse.Manager = warehouse.Manager;
            existingWarehouse.Status = warehouse.Status;
        }
    }

    public static bool DeleteWarehouse(int id)
    {
        var warehouse = MockWarehouses.FirstOrDefault(w => w.Id == id);
        if (warehouse != null)
        {
            return MockWarehouses.Remove(warehouse);
        }
        return false;
    }
}