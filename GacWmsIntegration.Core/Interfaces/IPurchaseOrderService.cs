using GacWmsIntegration.Core.Models;

namespace GacWmsIntegration.Core.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrder>> GetAllPurchaseOrdersAsync();
        Task<PurchaseOrder> GetPurchaseOrderByIdAsync(int orderId);
        Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersByCustomerAsync(int customerId);
        Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder purchaseOrder);
        Task<PurchaseOrder> UpdatePurchaseOrderAsync(PurchaseOrder purchaseOrder);
        Task<bool> DeletePurchaseOrderAsync(int orderId);
        Task<bool> PurchaseOrderExistsAsync(int orderId);
        Task<bool> ValidatePurchaseOrderAsync(PurchaseOrder purchaseOrder);

        // Purchase Order Details methods
        Task<PurchaseOrderDetails> AddOrderItemAsync(int orderId, PurchaseOrderDetails item);
        Task<bool> RemoveOrderItemAsync(int orderDetailId);
        Task<IEnumerable<PurchaseOrderDetails>> GetOrderItemsAsync(int orderId);
    }
}
