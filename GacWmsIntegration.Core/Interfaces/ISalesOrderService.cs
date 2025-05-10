using GacWmsIntegration.Core.Models;

namespace GacWmsIntegration.Core.Interfaces
{
    public interface ISalesOrderService
    {
        Task<IEnumerable<SalesOrder>> GetAllSalesOrdersAsync();
        Task<SalesOrder> GetSalesOrderByIdAsync(int orderId);
        Task<IEnumerable<SalesOrder>> GetSalesOrdersByCustomerAsync(int customerId);
        Task<SalesOrder> CreateSalesOrderAsync(SalesOrder salesOrder);
        Task<SalesOrder> UpdateSalesOrderAsync(SalesOrder salesOrder);
        Task<bool> DeleteSalesOrderAsync(int orderId);
        Task<bool> SalesOrderExistsAsync(int orderId);
        Task<bool> ValidateSalesOrderAsync(SalesOrder salesOrder);
        Task<bool> SyncSalesOrderWithWmsAsync(int orderId);

        // Sales Order Details methods
        Task<SalesOrderDetails> AddOrderItemAsync(int orderId, SalesOrderDetails item);
        Task<bool> RemoveOrderItemAsync(int orderDetailId);
        Task<IEnumerable<SalesOrderDetails>> GetOrderItemsAsync(int orderId);
    }
}
