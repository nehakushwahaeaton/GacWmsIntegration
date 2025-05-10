using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GacWmsIntegration.Core.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<PurchaseOrderService> _logger;

        public PurchaseOrderService(
            IApplicationDbContext dbContext,
            ILogger<PurchaseOrderService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllPurchaseOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all purchase orders");
                return await _dbContext.PurchaseOrders
                    .Include(po => po.Customer)
                    .Include(po => po.Items)
                        .ThenInclude(item => item.Product)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all purchase orders");
                throw;
            }
        }

        public async Task<PurchaseOrder> GetPurchaseOrderByIdAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Retrieving purchase order with ID: {OrderId}", orderId);
                var purchaseOrder = await _dbContext.PurchaseOrders
                    .Include(po => po.Customer)
                    .Include(po => po.Items)
                        .ThenInclude(item => item.Product)
                    .FirstOrDefaultAsync(po => po.OrderID == orderId);

                if (purchaseOrder == null)
                {
                    _logger.LogWarning("Purchase order with ID: {OrderId} not found", orderId);
                    return null!;
                }

                return purchaseOrder;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving purchase order with ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersByCustomerAsync(int customerId)
        {
            try
            {
                _logger.LogInformation("Retrieving purchase orders for customer ID: {CustomerId}", customerId);

                // Check if customer exists
                bool customerExists = await _dbContext.Customers.AnyAsync(c => c.CustomerID == customerId);
                if (!customerExists)
                {
                    _logger.LogWarning("Customer with ID: {CustomerId} not found", customerId);
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found");
                }

                return await _dbContext.PurchaseOrders
                    .Include(po => po.Customer)
                    .Include(po => po.Items)
                        .ThenInclude(item => item.Product)
                    .Where(po => po.CustomerID == customerId)
                    .ToListAsync();
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving purchase orders for customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                throw new ArgumentNullException(nameof(purchaseOrder));
            }

            try
            {
                // Validate purchase order data
                if (!await ValidatePurchaseOrderAsync(purchaseOrder))
                {
                    throw new InvalidOperationException("Purchase order validation failed");
                }

                // Set audit fields
                purchaseOrder.CreatedDate = DateTime.UtcNow;
                purchaseOrder.CreatedBy = Environment.UserName; // Or get from authentication context

                // Add to database
                await _dbContext.PurchaseOrders.AddAsync(purchaseOrder);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Purchase order created successfully with ID: {OrderId}", purchaseOrder.OrderID);

                return purchaseOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order for customer ID: {CustomerId}", purchaseOrder.CustomerID);
                throw;
            }
        }

        public async Task<PurchaseOrder> UpdatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                throw new ArgumentNullException(nameof(purchaseOrder));
            }

            try
            {
                // Check if purchase order exists
                var existingOrder = await _dbContext.PurchaseOrders
                    .Include(po => po.Items)
                    .FirstOrDefaultAsync(po => po.OrderID == purchaseOrder.OrderID);

                if (existingOrder == null)
                {
                    _logger.LogWarning("Purchase order with ID: {OrderId} not found for update", purchaseOrder.OrderID);
                    throw new KeyNotFoundException($"Purchase order with ID {purchaseOrder.OrderID} not found");
                }

                // Validate purchase order data
                if (!await ValidatePurchaseOrderAsync(purchaseOrder))
                {
                    throw new InvalidOperationException("Purchase order validation failed");
                }

                // Update basic properties
                existingOrder.ProcessingDate = purchaseOrder.ProcessingDate;
                existingOrder.CustomerID = purchaseOrder.CustomerID;

                // Preserve created audit fields
                purchaseOrder.CreatedDate = existingOrder.CreatedDate;
                purchaseOrder.CreatedBy = existingOrder.CreatedBy;

                // Save changes
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Purchase order updated successfully with ID: {OrderId}", purchaseOrder.OrderID);

                return existingOrder;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error updating purchase order with ID: {OrderId}", purchaseOrder.OrderID);
                throw;
            }
        }

        public async Task<bool> DeletePurchaseOrderAsync(int orderId)
        {
            try
            {
                // Check if purchase order exists
                var purchaseOrder = await _dbContext.PurchaseOrders
                    .Include(po => po.Items)
                    .FirstOrDefaultAsync(po => po.OrderID == orderId);

                if (purchaseOrder == null)
                {
                    _logger.LogWarning("Purchase order with ID: {OrderId} not found for deletion", orderId);
                    return false;
                }

                // Remove from database (cascade delete will handle order items)
                _dbContext.PurchaseOrders.Remove(purchaseOrder);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Purchase order deleted successfully with ID: {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase order with ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> PurchaseOrderExistsAsync(int orderId)
        {
            try
            {
                return await _dbContext.PurchaseOrders.AnyAsync(po => po.OrderID == orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if purchase order exists with ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> ValidatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                return false;
            }

            try
            {
                // Check if customer exists
                bool customerExists = await _dbContext.Customers.AnyAsync(c => c.CustomerID == purchaseOrder.CustomerID);
                if (!customerExists)
                {
                    _logger.LogWarning("Purchase order validation failed: Customer with ID {CustomerId} does not exist", purchaseOrder.CustomerID);
                    return false;
                }

                // Check if order has items
                if (purchaseOrder.Items == null || !purchaseOrder.Items.Any())
                {
                    _logger.LogWarning("Purchase order validation failed: Order must have at least one item");
                    return false;
                }

                // Validate each order item
                foreach (var item in purchaseOrder.Items)
                {
                    // Check if product exists
                    bool productExists = await _dbContext.Products.AnyAsync(p => p.ProductCode == item.ProductCode);
                    if (!productExists)
                    {
                        _logger.LogWarning("Purchase order validation failed: Product with code {ProductCode} does not exist", item.ProductCode);
                        return false;
                    }

                    // Check quantity
                    if (item.Quantity <= 0)
                    {
                        _logger.LogWarning("Purchase order validation failed: Item quantity must be greater than zero");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating purchase order");
                throw;
            }
        }

        public async Task<PurchaseOrderDetails> AddOrderItemAsync(int orderId, PurchaseOrderDetails item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            try
            {
                // Check if purchase order exists
                var purchaseOrder = await _dbContext.PurchaseOrders.FindAsync(orderId);
                if (purchaseOrder == null)
                {
                    _logger.LogWarning("Purchase order with ID: {OrderId} not found", orderId);
                    throw new KeyNotFoundException($"Purchase order with ID {orderId} not found");
                }

                // Check if product exists
                var product = await _dbContext.Products.FindAsync(item.ProductCode);
                if (product == null)
                {
                    _logger.LogWarning("Product with code: {ProductCode} not found", item.ProductCode);
                    throw new KeyNotFoundException($"Product with code {item.ProductCode} not found");
                }

                // Set order ID and created date
                item.OrderID = orderId;
                item.CreatedDate = DateTime.UtcNow;

                // Add to database
                await _dbContext.PurchaseOrderDetails.AddAsync(item);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Order item added successfully to purchase order ID: {OrderId}", orderId);

                return item;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error adding order item to purchase order ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> RemoveOrderItemAsync(int orderDetailId)
        {
            try
            {
                // Check if order item exists
                var orderItem = await _dbContext.PurchaseOrderDetails.FindAsync(orderDetailId);
                if (orderItem == null)
                {
                    _logger.LogWarning("Order item with ID: {OrderDetailId} not found", orderDetailId);
                    return false;
                }

                // Store order ID for synchronization
                int orderId = orderItem.OrderID;

                // Remove from database
                _dbContext.PurchaseOrderDetails.Remove(orderItem);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Order item removed successfully with ID: {OrderDetailId}", orderDetailId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing order item with ID: {OrderDetailId}", orderDetailId);
                throw;
            }
        }

        public async Task<IEnumerable<PurchaseOrderDetails>> GetOrderItemsAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Retrieving order items for purchase order ID: {OrderId}", orderId);

                // Check if purchase order exists
                bool orderExists = await _dbContext.PurchaseOrders.AnyAsync(po => po.OrderID == orderId);
                if (!orderExists)
                {
                    _logger.LogWarning("Purchase order with ID: {OrderId} not found", orderId);
                    throw new KeyNotFoundException($"Purchase order with ID {orderId} not found");
                }

                return await _dbContext.PurchaseOrderDetails
                    .Include(item => item.Product)
                    .Where(item => item.OrderID == orderId)
                    .ToListAsync();
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving order items for purchase order ID: {OrderId}", orderId);
                throw;
            }
        }
    }
}
