using GacWmsIntegration.Core.Models;

namespace GacWmsIntegration.Core.Interfaces
{
    public interface IWmsService
    {
        // Synchronization methods for all entity types
        Task<bool> SynchronizeCustomerAsync(Customer customer);
        Task<bool> SynchronizeProductAsync(Product product);
        Task<bool> SynchronizePurchaseOrderAsync(PurchaseOrder purchaseOrder);
        Task<bool> SynchronizeSalesOrderAsync(SalesOrder salesOrder);

        // Batch synchronization methods
        Task<IEnumerable<SyncResult>> SynchronizeCustomersAsync(IEnumerable<int> customerIds);
        Task<IEnumerable<SyncResult>> SynchronizeProductsAsync(IEnumerable<string> productCodes);
        Task<IEnumerable<SyncResult>> SynchronizePurchaseOrdersAsync(IEnumerable<int> orderIds);
        Task<IEnumerable<SyncResult>> SynchronizeSalesOrdersAsync(IEnumerable<int> orderIds);

        // Retry failed synchronizations
        Task<IEnumerable<SyncResult>> RetryFailedSynchronizationsAsync();

        // Get synchronization status
        Task<SyncStatus> GetSyncStatusAsync(string entityType, string entityId);
    }

    // Helper classes for the WMS service
    public class SyncResult
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime SyncDate { get; set; } = DateTime.UtcNow;
    }

    public class SyncStatus
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Pending", "Synced", "Failed"
        public DateTime? LastSyncDate { get; set; }
        public int RetryCount { get; set; }
        public string LastErrorMessage { get; set; } = string.Empty;
    }
}
