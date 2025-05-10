using GacWmsIntegration.Core.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;

public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    DbSet<PurchaseOrderDetails> PurchaseOrderDetails { get; set; }
    DbSet<SalesOrder> SalesOrders { get; set; }
    DbSet<SalesOrderDetails> SalesOrderDetails { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Add this method to expose Entry functionality
    EntityEntry Entry(object entity);
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
