using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GacWmsIntegration.Core.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IWmsApiClient _wmsApiClient;
        private readonly ILogger<SalesOrderService> _logger;

        public SalesOrderService(
            IApplicationDbContext dbContext,
            IWmsApiClient wmsApiClient,
            ILogger<SalesOrderService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _wmsApiClient = wmsApiClient ?? throw new ArgumentNullException(nameof(wmsApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<SalesOrder>> GetAllSalesOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all sales orders");
                return await _dbContext.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Items)
                        .ThenInclude(item => item.Product)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all sales orders");
                throw;
            }
        }

        public async Task<SalesOrder> GetSalesOrderByIdAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Retrieving sales order with ID: {OrderId}", orderId);
                var salesOrder = await _dbContext.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Items)
                        .ThenInclude(item => item.Product)
                    .FirstOrDefaultAsync(so => so.OrderID == orderId);

                if (salesOrder == null)
                {
                    _logger.LogWarning("Sales order with ID: {OrderId} not found", orderId);
                    throw new KeyNotFoundException($"Sales order with ID {orderId} not found");
                }

                return salesOrder;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving sales order with ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<SalesOrder>> GetSalesOrdersByCustomerAsync(int customerId)
        {
            try
            {
                _logger.LogInformation("Retrieving sales orders for customer ID: {CustomerId}", customerId);

                // Check if customer exists
                bool customerExists = await _dbContext.Customers.AnyAsync(c => c.CustomerID == customerId);
                if (!customerExists)
                {
                    _logger.LogWarning("Customer with ID: {CustomerId} not found", customerId);
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found");
                }

                return await _dbContext.SalesOrders
                    .Include(so => so.Customer)
                    .Include(so => so.Items)
                        .ThenInclude(item => item.Product)
                    .Where(so => so.CustomerID == customerId)
                    .ToListAsync();
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving sales orders for customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<SalesOrder> CreateSalesOrderAsync(SalesOrder salesOrder)
        {
            if (salesOrder == null)
            {
                throw new ArgumentNullException(nameof(salesOrder));
            }

            try
            {
                // Validate sales order data
                if (!await ValidateSalesOrderAsync(salesOrder))
                {
                    throw new InvalidOperationException("Sales order validation failed");
                }

                // Set audit fields
                salesOrder.CreatedDate = DateTime.UtcNow;
                salesOrder.CreatedBy = Environment.UserName; // Or get from authentication context

                // Add to database
                await _dbContext.SalesOrders.AddAsync(salesOrder);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Sales order created successfully with ID: {OrderId}", salesOrder.OrderID);

                // Synchronize with WMS
                await SyncSalesOrderWithWmsAsync(salesOrder.OrderID);

                return salesOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sales order for customer ID: {CustomerId}", salesOrder.CustomerID);
                throw;
            }
        }

        public async Task<SalesOrder> UpdateSalesOrderAsync(SalesOrder salesOrder)
        {
            if (salesOrder == null)
            {
                throw new ArgumentNullException(nameof(salesOrder));
            }

            try
            {
                // Check if sales order exists
                var existingOrder = await _dbContext.SalesOrders
                    .Include(so => so.Items)
                    .FirstOrDefaultAsync(so => so.OrderID == salesOrder.OrderID);

                if (existingOrder == null)
                {
                    _logger.LogWarning("Sales order with ID: {OrderId} not found for update", salesOrder.OrderID);
                    throw new KeyNotFoundException($"Sales order with ID {salesOrder.OrderID} not found");
                }

                // Validate sales order data
                if (!await ValidateSalesOrderAsync(salesOrder))
                {
                    throw new InvalidOperationException("Sales order validation failed");
                }

                // Update basic properties
                existingOrder.ProcessingDate = salesOrder.ProcessingDate;
                existingOrder.CustomerID = salesOrder.CustomerID;
                existingOrder.ShipmentAddress = salesOrder.ShipmentAddress;

                // Preserve created audit fields
                salesOrder.CreatedDate = existingOrder.CreatedDate;
                salesOrder.CreatedBy = existingOrder.CreatedBy;

                // Save changes
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Sales order updated successfully with ID: {OrderId}", salesOrder.OrderID);

                // Synchronize with WMS
                await SyncSalesOrderWithWmsAsync(salesOrder.OrderID);

                return existingOrder;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error updating sales order with ID: {OrderId}", salesOrder.OrderID);
                throw;
            }
        }

        public async Task<bool> DeleteSalesOrderAsync(int orderId)
        {
            try
            {
                // Check if sales order exists
                var salesOrder = await _dbContext.SalesOrders
                    .Include(so => so.Items)
                    .FirstOrDefaultAsync(so => so.OrderID == orderId);

                if (salesOrder == null)
                {
                    _logger.LogWarning("Sales order with ID: {OrderId} not found for deletion", orderId);
                    return false;
                }

                // Remove from database (cascade delete will handle order items)
                _dbContext.SalesOrders.Remove(salesOrder);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Sales order deleted successfully with ID: {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sales order with ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> SalesOrderExistsAsync(int orderId)
        {
            try
            {
                return await _dbContext.SalesOrders.AnyAsync(so => so.OrderID == orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if sales order exists with ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> ValidateSalesOrderAsync(SalesOrder salesOrder)
        {
            if (salesOrder == null)
            {
                return false;
            }

            try
            {
                // Check if customer exists
                bool customerExists = await _dbContext.Customers.AnyAsync(c => c.CustomerID == salesOrder.CustomerID);
                if (!customerExists)
                {
                    _logger.LogWarning("Sales order validation failed: Customer with ID {CustomerId} does not exist", salesOrder.CustomerID);
                    return false;
                }

                // Check if shipment address is provided
                if (string.IsNullOrWhiteSpace(salesOrder.ShipmentAddress))
                {
                    _logger.LogWarning("Sales order validation failed: Shipment address is required");
                    return false;
                }

                // Check if order has items
                if (salesOrder.Items == null || !salesOrder.Items.Any())
                {
                    _logger.LogWarning("Sales order validation failed: Order must have at least one item");
                    return false;
                }

                // Validate each order item
                foreach (var item in salesOrder.Items)
                {
                    // Check if product exists
                    bool productExists = await _dbContext.Products.AnyAsync(p => p.ProductCode == item.ProductCode);
                    if (!productExists)
                    {
                        _logger.LogWarning("Sales order validation failed: Product with code {ProductCode} does not exist", item.ProductCode);
                        return false;
                    }

                    // Check quantity
                    if (item.Quantity <= 0)
                    {
                        _logger.LogWarning("Sales order validation failed: Item quantity must be greater than zero");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating sales order");
                throw;
            }
        }

        public async Task<bool> SyncSalesOrderWithWmsAsync(int orderId)
        {
            try
            {
                // Get sales order from database with all related data
                var salesOrder = await GetSalesOrderByIdAsync(orderId);

                // Send sales order to WMS
                bool result = await _wmsApiClient.SendSalesOrderAsync(salesOrder);

                if (result)
                {
                    _logger.LogInformation("Sales order synchronized successfully with WMS. Order ID: {OrderId}", orderId);
                }
                else
                {
                    _logger.LogWarning("Failed to synchronize sales order with WMS. Order ID: {OrderId}", orderId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing sales order with WMS. Order ID: {OrderId}", orderId);
                return false;
            }
        }

        public async Task<SalesOrderDetails> AddOrderItemAsync(int orderId, SalesOrderDetails item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            try
            {
                // Check if sales order exists
                var salesOrder = await _dbContext.SalesOrders.FindAsync(orderId);
                if (salesOrder == null)
                {
                    _logger.LogWarning("Sales order with ID: {OrderId} not found", orderId);
                    throw new KeyNotFoundException($"Sales order with ID {orderId} not found");
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
                await _dbContext.SalesOrderDetails.AddAsync(item);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Order item added successfully to sales order ID: {OrderId}", orderId);

                // Synchronize updated order with WMS
                await SyncSalesOrderWithWmsAsync(orderId);

                return item;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error adding order item to sales order ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> RemoveOrderItemAsync(int orderDetailId)
        {
            try
            {
                // Check if order item exists
                var orderItem = await _dbContext.SalesOrderDetails.FindAsync(orderDetailId);
                if (orderItem == null)
                {
                    _logger.LogWarning("Order item with ID: {OrderDetailId} not found", orderDetailId);
                    return false;
                }

                // Store order ID for synchronization
                int orderId = orderItem.OrderID;

                // Remove from database
                _dbContext.SalesOrderDetails.Remove(orderItem);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Order item removed successfully with ID: {OrderDetailId}", orderDetailId);

                // Synchronize updated order with WMS
                await SyncSalesOrderWithWmsAsync(orderId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing order item with ID: {OrderDetailId}", orderDetailId);
                throw;
            }
        }

        public async Task<IEnumerable<SalesOrderDetails>> GetOrderItemsAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Retrieving order items for sales order ID: {OrderId}", orderId);

                // Check if sales order exists
                bool orderExists = await _dbContext.SalesOrders.AnyAsync(so => so.OrderID == orderId);
                if (!orderExists)
                {
                    _logger.LogWarning("Sales order with ID: {OrderId} not found", orderId);
                    throw new KeyNotFoundException($"Sales order with ID {orderId} not found");
                }

                return await _dbContext.SalesOrderDetails
                    .Include(item => item.Product)
                    .Where(item => item.OrderID == orderId)
                    .ToListAsync();
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving order items for sales order ID: {OrderId}", orderId);
                throw;
            }
        }
    }
}
