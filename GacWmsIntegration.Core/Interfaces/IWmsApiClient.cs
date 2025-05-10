using GacWmsIntegration.Core.Models;

namespace GacWmsIntegration.Core.Interfaces
{
    public interface IWmsApiClient
    {
        // Customer operations
        Task<bool> SendCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(Customer customer);

        // Product operations
        Task<bool> SendProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);

        // Purchase Order operations
        Task<bool> SendPurchaseOrderAsync(PurchaseOrder purchaseOrder);
        Task<bool> UpdatePurchaseOrderStatusAsync(int orderId, string status);

        // Sales Order operations
        Task<bool> SendSalesOrderAsync(SalesOrder salesOrder);
        Task<bool> UpdateSalesOrderStatusAsync(int orderId, string status);

        // System operations
        Task<bool> PingWmsAsync();
        Task<string> GetWmsVersionAsync();
    }
}
