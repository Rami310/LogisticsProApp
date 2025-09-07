using Microsoft.EntityFrameworkCore;
using LogisticsPro.API.Models;

namespace LogisticsPro.API.Data
{
    public class LogisticsDbContext : DbContext
    {
        public LogisticsDbContext(DbContextOptions<LogisticsDbContext> options) : base(options)
        {
        }

        // DbSets for all your entities
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<ProductRequest> ProductRequests { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        
        // Revenue system tables
        public DbSet<CompanyRevenue> CompanyRevenue { get; set; }
        public DbSet<RevenueTransaction> RevenueTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).HasMaxLength(50);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");

                // Foreign key relationship with Department
                entity.HasOne<Department>()
                      .WithMany()
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.UnitOfMeasure).HasMaxLength(50);
                entity.Property(e => e.Supplier).HasMaxLength(200);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");

                // Unique constraint on SKU
                entity.HasIndex(e => e.SKU).IsUnique();
            });

            // Configure InventoryItem entity
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Location).HasMaxLength(50);

                // Foreign key relationships
                entity.HasOne<Product>()
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Warehouse>()
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ProductRequest entity - FIXED: Only one ProductId relationship
            modelBuilder.Entity<ProductRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RequestedBy).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RequestStatus).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.ApprovedBy).HasMaxLength(50);
                entity.Property(e => e.ReceivedBy).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.TotalCost).HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.Property(e => e.CreatedBy).HasMaxLength(50);

                // ONLY ONE foreign key relationship to Product
                entity.HasOne(r => r.Product)
                    .WithMany()
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure Department entity - FIXED with proper value comparer
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // Convert AllowedRoles list to JSON string for storage with proper comparer
                entity.Property(e => e.AllowedRoles)
                    .HasConversion(
                        v => string.Join(',', v ?? new List<string>()),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                    .Metadata.SetValueComparer(
                        new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToList()));
            });

            // Configure Warehouse entity
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Manager).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            });
            
            
            // Configure CompanyRevenue entity - FIXED: No NOW() defaults
            modelBuilder.Entity<CompanyRevenue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CurrentRevenue).HasColumnType("decimal(15,2)").IsRequired();
                entity.Property(e => e.AvailableBudget).HasColumnType("decimal(15,2)").IsRequired();
                entity.Property(e => e.TotalSpent).HasColumnType("decimal(15,2)").HasDefaultValue(0);
                entity.Property(e => e.UpdatedBy).HasMaxLength(50);
                entity.Property(e => e.UpdateReason).HasMaxLength(255);
                
                // Don't use NOW() - let Entity Framework handle it in C#
                // entity.Property(e => e.LastUpdated) - no default, handled by C# DateTime.Now
            });

            // Configure RevenueTransaction entity - FIXED: No NOW() defaults
            modelBuilder.Entity<RevenueTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Amount).HasColumnType("decimal(15,2)").IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.BalanceAfter).HasColumnType("decimal(15,2)");
            
                // Don't use NOW() - let Entity Framework handle it in C#
                // entity.Property(e => e.CreatedDate) - no default, handled by C# DateTime.Now
            
                // Foreign key to ProductRequests
                entity.HasOne(e => e.ProductRequest)
                    .WithMany()
                    .HasForeignKey(e => e.ProductRequestId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}