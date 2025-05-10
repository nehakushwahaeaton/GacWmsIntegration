using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using GacWmsIntegration.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GacWmsIntegration.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ILogger<ApplicationDbContext> logger) : base(options)
        {
            _logger = logger;
        }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;
        public DbSet<PurchaseOrderDetails> PurchaseOrderDetails { get; set; } = null!;
        public DbSet<SalesOrder> SalesOrders { get; set; } = null!;
        public DbSet<SalesOrderDetails> SalesOrderDetails { get; set; } = null!;

        // Entity sets for synchronization tracking
        public DbSet<SyncResultEntity> SyncResults { get; set; }
        public DbSet<SyncStatusEntity> SyncStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasKey(p => p.ProductCode);

            modelBuilder.Entity<Customer>()
                .HasKey(c => c.CustomerID);

            modelBuilder.Entity<PurchaseOrder>()
                .HasKey(po => po.OrderID);

            modelBuilder.Entity<PurchaseOrderDetails>()
                .HasKey(pod => pod.OrderDetailID);

            modelBuilder.Entity<SalesOrder>()
                .HasKey(so => so.OrderID);

            modelBuilder.Entity<SalesOrderDetails>()
                .HasKey(sod => sod.OrderDetailID);

            // Configure SyncResultEntity
            modelBuilder.Entity<SyncResultEntity>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<SyncResultEntity>()
                .Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<SyncResultEntity>()
                .Property(e => e.EntityId)
                .IsRequired()
                .HasMaxLength(50);

            // Configure SyncStatusEntity
            modelBuilder.Entity<SyncStatusEntity>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<SyncStatusEntity>()
                .Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<SyncStatusEntity>()
                .Property(e => e.EntityId)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<SyncStatusEntity>()
                .Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);

            // Create a unique index on EntityType and EntityId for SyncStatusEntity
            modelBuilder.Entity<SyncStatusEntity>()
                .HasIndex(e => new { e.EntityType, e.EntityId })
                .IsUnique();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Update audit fields before saving
                UpdateAuditFields();

                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving changes to the database");
                throw;
            }
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Configure Customer entity
        //    modelBuilder.Entity<Customer>(entity =>
        //    {
        //        entity.HasKey(e => e.CustomerID);
        //        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        //        entity.Property(e => e.Address).IsRequired().HasMaxLength(255);
        //        entity.Property(e => e.CreatedBy).HasMaxLength(50);
        //        entity.Property(e => e.ModifiedBy).HasMaxLength(50);
        //    });

        //    // Configure Product entity
        //    modelBuilder.Entity<Product>(entity =>
        //    {
        //        entity.HasKey(e => e.ProductCode);
        //        entity.Property(e => e.ProductCode).HasMaxLength(50);
        //        entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
        //        entity.Property(e => e.Description).HasMaxLength(255);
        //        entity.Property(e => e.Dimensions).HasMaxLength(100);
        //        entity.Property(e => e.CreatedBy).HasMaxLength(50);
        //        entity.Property(e => e.ModifiedBy).HasMaxLength(50);
        //    });

        //    // Configure PurchaseOrder entity
        //    modelBuilder.Entity<PurchaseOrder>(entity =>
        //    {
        //        entity.HasKey(e => e.OrderID);
        //        entity.Property(e => e.CreatedBy).HasMaxLength(50);

        //        // Configure relationship with Customer
        //        entity.HasOne(e => e.Customer)
        //              .WithMany(c => c.PurchaseOrders)
        //              .HasForeignKey(e => e.CustomerID)
        //              .OnDelete(DeleteBehavior.Restrict);
        //    });

        //    // Configure PurchaseOrderDetails entity
        //    modelBuilder.Entity<PurchaseOrderDetails>(entity =>
        //    {
        //        entity.HasKey(e => e.OrderDetailID);

        //        // Configure relationship with PurchaseOrder
        //        entity.HasOne(e => e.PurchaseOrder)
        //              .WithMany(po => po.Items)
        //              .HasForeignKey(e => e.OrderID)
        //              .OnDelete(DeleteBehavior.Cascade);

        //        // Configure relationship with Product
        //        entity.HasOne(e => e.Product)
        //              .WithMany()
        //              .HasForeignKey(e => e.ProductCode)
        //              .OnDelete(DeleteBehavior.Restrict);
        //    });

        //    // Configure SalesOrder entity
        //    modelBuilder.Entity<SalesOrder>(entity =>
        //    {
        //        entity.HasKey(e => e.OrderID);
        //        entity.Property(e => e.ShipmentAddress).IsRequired().HasMaxLength(255);
        //        entity.Property(e => e.CreatedBy).HasMaxLength(50);

        //        // Configure relationship with Customer
        //        entity.HasOne(e => e.Customer)
        //              .WithMany(c => c.SalesOrders)
        //              .HasForeignKey(e => e.CustomerID)
        //              .OnDelete(DeleteBehavior.Restrict);
        //    });

        //    // Configure SalesOrderDetails entity
        //    modelBuilder.Entity<SalesOrderDetails>(entity =>
        //    {
        //        entity.HasKey(e => e.OrderDetailID);

        //        // Configure relationship with SalesOrder
        //        entity.HasOne(e => e.SalesOrder)
        //              .WithMany(so => so.Items)
        //              .HasForeignKey(e => e.OrderID)
        //              .OnDelete(DeleteBehavior.Cascade);

        //        // Configure relationship with Product
        //        entity.HasOne(e => e.Product)
        //              .WithMany()
        //              .HasForeignKey(e => e.ProductCode)
        //              .OnDelete(DeleteBehavior.Restrict);
        //    });
        //}

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries();
            var currentUsername = Environment.UserName; // Or get from authentication context
            var currentTime = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                // Update CreatedDate and CreatedBy for new entities
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is Customer customer)
                    {
                        customer.CreatedDate = currentTime;
                        customer.CreatedBy = currentUsername;
                        customer.ModifiedDate = currentTime;
                        customer.ModifiedBy = currentUsername;
                    }
                    else if (entry.Entity is Product product)
                    {
                        product.CreatedDate = currentTime;
                        product.CreatedBy = currentUsername;
                        product.ModifiedDate = currentTime;
                        product.ModifiedBy = currentUsername;
                    }
                    else if (entry.Entity is PurchaseOrder purchaseOrder)
                    {
                        purchaseOrder.CreatedDate = currentTime;
                        purchaseOrder.CreatedBy = currentUsername;
                    }
                    else if (entry.Entity is PurchaseOrderDetails purchaseOrderDetails)
                    {
                        purchaseOrderDetails.CreatedDate = currentTime;
                    }
                    else if (entry.Entity is SalesOrder salesOrder)
                    {
                        salesOrder.CreatedDate = currentTime;
                        salesOrder.CreatedBy = currentUsername;
                    }
                    else if (entry.Entity is SalesOrderDetails salesOrderDetails)
                    {
                        salesOrderDetails.CreatedDate = currentTime;
                    }
                }
                // Update ModifiedDate and ModifiedBy for modified entities
                else if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity is Customer customer)
                    {
                        customer.ModifiedDate = currentTime;
                        customer.ModifiedBy = currentUsername;
                    }
                    else if (entry.Entity is Product product)
                    {
                        product.ModifiedDate = currentTime;
                        product.ModifiedBy = currentUsername;
                    }
                }
            }
        }
    }
}
