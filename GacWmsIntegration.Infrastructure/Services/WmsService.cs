using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.Extensions.Logging;

namespace GacWmsIntegration.Infrastructure.Services
{
    public class WmsService : IWmsService
    {
        private readonly IWmsApiClient _wmsApiClient;
        private readonly ISyncRepository _syncRepository;
        private readonly ICustomerService _customerRepository;
        private readonly IProductService _productRepository;
        private readonly IPurchaseOrderService _purchaseOrderRepository;
        private readonly ISalesOrderService _salesOrderRepository;
        private readonly ILogger<WmsService> _logger;

        public WmsService(
            IWmsApiClient wmsApiClient,
            ISyncRepository syncRepository,
            ICustomerService customerRepository,
            IProductService productRepository,
            IPurchaseOrderService purchaseOrderRepository,
            ISalesOrderService salesOrderRepository,
            ILogger<WmsService> logger)
        {
            _wmsApiClient = wmsApiClient ?? throw new ArgumentNullException(nameof(wmsApiClient));
            _syncRepository = syncRepository ?? throw new ArgumentNullException(nameof(syncRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _purchaseOrderRepository = purchaseOrderRepository ?? throw new ArgumentNullException(nameof(purchaseOrderRepository));
            _salesOrderRepository = salesOrderRepository ?? throw new ArgumentNullException(nameof(salesOrderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SynchronizeCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            string entityId = customer.CustomerID.ToString();
            string entityType = "Customer";

            try
            {
                _logger.LogInformation("Synchronizing customer with WMS: {CustomerId}", entityId);

                bool success = await _wmsApiClient.SendCustomerAsync(customer);

                // Record synchronization result
                var syncResult = new SyncResult
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Success = success,
                    ErrorMessage = success ? string.Empty : "Failed to synchronize customer with WMS",
                    SyncDate = DateTime.UtcNow
                };

                await _syncRepository.RecordSyncResultAsync(syncResult);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing customer with WMS: {CustomerId}", entityId);

                // Record synchronization failure
                var syncResult = new SyncResult
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Success = false,
                    ErrorMessage = ex.Message,
                    SyncDate = DateTime.UtcNow
                };

                await _syncRepository.RecordSyncResultAsync(syncResult);

                return false;
            }
        }

        public async Task<bool> SynchronizeProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            string entityId = product.ProductCode;
            string entityType = "Product";

            try
            {
                _logger.LogInformation("Synchronizing product with WMS: {ProductCode}", entityId);

                bool success = await _wmsApiClient.SendProductAsync(product);

                // Record synchronization result
                var syncResult = new SyncResult
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Success = success,
                    ErrorMessage = success ? string.Empty : "Failed to synchronize product with WMS",
                    SyncDate = DateTime.UtcNow
                };

                await _syncRepository.RecordSyncResultAsync(syncResult);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing product with WMS: {ProductCode}", entityId);

                // Record synchronization failure
                var syncResult = new SyncResult
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Success = false,
                    ErrorMessage = ex.Message,
                    SyncDate = DateTime.UtcNow
                };

                await _syncRepository.RecordSyncResultAsync(syncResult);

                return false;
            }
        }

        public async Task<bool> SynchronizePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                throw new ArgumentNullException(nameof(purchaseOrder));
            }

            string entityId = purchaseOrder.OrderID.ToString();
            string entityType = "PurchaseOrder";

            try
            {
                _logger.LogInformation("Synchronizing purchase order with WMS: {OrderId}", entityId);

                // Ensure all related entities are synchronized first
                if (purchaseOrder.Customer != null)
                {
                    await SynchronizeCustomerAsync(purchaseOrder.Customer);
                }

                foreach (var item in purchaseOrder.Items)
                {
                    var product = await _productRepository.GetProductByCodeAsync(item.ProductCode);
                    if (product != null)
                    {
                        await SynchronizeProductAsync(product);
                    }
                }

                bool success = await _wmsApiClient.SendPurchaseOrderAsync(purchaseOrder);

                // Record synchronization result
                var syncResult = new SyncResult
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Success = success,
                    ErrorMessage = success ? string.Empty : "Failed to synchronize purchase order with WMS",
                    SyncDate = DateTime.UtcNow
                };

                await _syncRepository.RecordSyncResultAsync(syncResult);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing purchase order with WMS: {OrderId}", entityId);

                // Record synchronization failure
                var syncResult = new SyncResult
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Success = false,
                    ErrorMessage = ex.Message,
                    SyncDate = DateTime.UtcNow
                };

                await _syncRepository.RecordSyncResultAsync(syncResult);

                return false;
            }
        }

        public async Task<bool> SynchronizeSalesOrderAsync(SalesOrder salesOrder)
        {
            if (salesOrder == null)
            {
                throw new ArgumentNullException(nameof(salesOrder));
            }

            string entityId = salesOrder.OrderID.ToString();
            string entityType = "SalesOrder";

            try
            {
                _logger.LogInformation("Synchronizing sales order with WMS: {OrderId}", entityId);

                // Ensure all related entities are synchronized first
                if (salesOrder.Customer != null)
                {
                    await SynchronizeCustomerAsync(salesOrder.Customer);
                }

                foreach (var item in salesOrder.Items)
                {
                    var product = await _productRepository.GetProductByCodeAsync(item.ProductCode);
                    if (product != null)
                    {
                        await SynchronizeProductAsync(product);
                    }
                }

                bool success = await _wmsApiClient.SendSalesOrderAsync(salesOrder);

                // Record synchronization result
                var syncResult = new SyncResult
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Success = success,
                    ErrorMessage = success ? string.Empty : "Failed to synchronize sales order with WMS",
                    SyncDate = DateTime.UtcNow
                };

                await _syncRepository.RecordSyncResultAsync(syncResult);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing sales order with WMS: {OrderId}", entityId);

                // Record synchronization failure
                var syncResult = new SyncResult
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Success = false,
                    ErrorMessage = ex.Message,
                    SyncDate = DateTime.UtcNow
                };

                await _syncRepository.RecordSyncResultAsync(syncResult);

                return false;
            }
        }

        public async Task<IEnumerable<SyncResult>> SynchronizeCustomersAsync(IEnumerable<int> customerIds)
        {
            if (customerIds == null)
            {
                throw new ArgumentNullException(nameof(customerIds));
            }

            var results = new List<SyncResult>();

            foreach (var customerId in customerIds)
            {
                try
                {
                    var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
                    if (customer == null)
                    {
                        results.Add(new SyncResult
                        {
                            EntityType = "Customer",
                            EntityId = customerId.ToString(),
                            Success = false,
                            ErrorMessage = $"Customer with ID {customerId} not found",
                            SyncDate = DateTime.UtcNow
                        });
                        continue;
                    }

                    bool success = await SynchronizeCustomerAsync(customer);
                    results.Add(new SyncResult
                    {
                        EntityType = "Customer",
                        EntityId = customerId.ToString(),
                        Success = success,
                        ErrorMessage = success ? string.Empty : "Failed to synchronize customer with WMS",
                        SyncDate = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error synchronizing customer with WMS: {CustomerId}", customerId);
                    results.Add(new SyncResult
                    {
                        EntityType = "Customer",
                        EntityId = customerId.ToString(),
                        Success = false,
                        ErrorMessage = ex.Message,
                        SyncDate = DateTime.UtcNow
                    });
                }
            }

            return results;
        }

        public async Task<IEnumerable<SyncResult>> SynchronizeProductsAsync(IEnumerable<string> productCodes)
        {
            if (productCodes == null)
            {
                throw new ArgumentNullException(nameof(productCodes));
            }

            var results = new List<SyncResult>();

            foreach (var productCode in productCodes)
            {
                try
                {
                    var product = await _productRepository.GetProductByCodeAsync(productCode);
                    if (product == null)
                    {
                        results.Add(new SyncResult
                        {
                            EntityType = "Product",
                            EntityId = productCode,
                            Success = false,
                            ErrorMessage = $"Product with code {productCode} not found",
                            SyncDate = DateTime.UtcNow
                        });
                        continue;
                    }

                    bool success = await SynchronizeProductAsync(product);
                    results.Add(new SyncResult
                    {
                        EntityType = "Product",
                        EntityId = productCode,
                        Success = success,
                        ErrorMessage = success ? string.Empty : "Failed to synchronize product with WMS",
                        SyncDate = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error synchronizing product with WMS: {ProductCode}", productCode);
                    results.Add(new SyncResult
                    {
                        EntityType = "Product",
                        EntityId = productCode,
                        Success = false,
                        ErrorMessage = ex.Message,
                        SyncDate = DateTime.UtcNow
                    });
                }
            }

            return results;
        }

        public async Task<IEnumerable<SyncResult>> SynchronizePurchaseOrdersAsync(IEnumerable<int> orderIds)
        {
            if (orderIds == null)
            {
                throw new ArgumentNullException(nameof(orderIds));
            }

            var results = new List<SyncResult>();

            foreach (var orderId in orderIds)
            {
                try
                {
                    var purchaseOrder = await _purchaseOrderRepository.GetPurchaseOrderByIdAsync(orderId);
                    if (purchaseOrder == null)
                    {
                        results.Add(new SyncResult
                        {
                            EntityType = "PurchaseOrder",
                            EntityId = orderId.ToString(),
                            Success = false,
                            ErrorMessage = $"Purchase order with ID {orderId} not found",
                            SyncDate = DateTime.UtcNow
                        });
                        continue;
                    }

                    bool success = await SynchronizePurchaseOrderAsync(purchaseOrder);
                    results.Add(new SyncResult
                    {
                        EntityType = "PurchaseOrder",
                        EntityId = orderId.ToString(),
                        Success = success,
                        ErrorMessage = success ? string.Empty : "Failed to synchronize purchase order with WMS",
                        SyncDate = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error synchronizing purchase order with WMS: {OrderId}", orderId);
                    results.Add(new SyncResult
                    {
                        EntityType = "PurchaseOrder",
                        EntityId = orderId.ToString(),
                        Success = false,
                        ErrorMessage = ex.Message,
                        SyncDate = DateTime.UtcNow
                    });
                }
            }

            return results;
        }

        public async Task<IEnumerable<SyncResult>> SynchronizeSalesOrdersAsync(IEnumerable<int> orderIds)
        {
            if (orderIds == null)
            {
                throw new ArgumentNullException(nameof(orderIds));
            }

            var results = new List<SyncResult>();

            foreach (var orderId in orderIds)
            {
                try
                {
                    var salesOrder = await _salesOrderRepository.GetSalesOrderByIdAsync(orderId);
                    if (salesOrder == null)
                    {
                        results.Add(new SyncResult
                        {
                            EntityType = "SalesOrder",
                            EntityId = orderId.ToString(),
                            Success = false,
                            ErrorMessage = $"Sales order with ID {orderId} not found",
                            SyncDate = DateTime.UtcNow
                        });
                        continue;
                    }

                    bool success = await SynchronizeSalesOrderAsync(salesOrder);
                    results.Add(new SyncResult
                    {
                        EntityType = "SalesOrder",
                        EntityId = orderId.ToString(),
                        Success = success,
                        ErrorMessage = success ? string.Empty : "Failed to synchronize sales order with WMS",
                        SyncDate = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error synchronizing sales order with WMS: {OrderId}", orderId);
                    results.Add(new SyncResult
                    {
                        EntityType = "SalesOrder",
                        EntityId = orderId.ToString(),
                        Success = false,
                        ErrorMessage = ex.Message,
                        SyncDate = DateTime.UtcNow
                    });
                }
            }

            return results;
        }

        public async Task<IEnumerable<SyncResult>> RetryFailedSynchronizationsAsync()
        {
            try
            {
                _logger.LogInformation("Retrying failed synchronizations");

                // Get all failed synchronizations
                var failedSyncs = await _syncRepository.GetFailedSynchronizationsAsync();
                var results = new List<SyncResult>();

                foreach (var syncStatus in failedSyncs)
                {
                    try
                    {
                        bool success = false;

                        switch (syncStatus.EntityType)
                        {
                            case "Customer":
                                if (int.TryParse(syncStatus.EntityId, out int customerId))
                                {
                                    var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
                                    if (customer != null)
                                    {
                                        success = await SynchronizeCustomerAsync(customer);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Customer not found for retry: {CustomerId}", customerId);
                                    }
                                }
                                break;

                            case "Product":
                                var product = await _productRepository.GetProductByCodeAsync(syncStatus.EntityId);
                                if (product != null)
                                {
                                    success = await SynchronizeProductAsync(product);
                                }
                                else
                                {
                                    _logger.LogWarning("Product not found for retry: {ProductCode}", syncStatus.EntityId);
                                }
                                break;

                            case "PurchaseOrder":
                                if (int.TryParse(syncStatus.EntityId, out int purchaseOrderId))
                                {
                                    var purchaseOrder = await _purchaseOrderRepository.GetPurchaseOrderByIdAsync(purchaseOrderId);
                                    if (purchaseOrder != null)
                                    {
                                        success = await SynchronizePurchaseOrderAsync(purchaseOrder);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Purchase order not found for retry: {OrderId}", purchaseOrderId);
                                    }
                                }
                                break;

                            case "SalesOrder":
                                if (int.TryParse(syncStatus.EntityId, out int salesOrderId))
                                {
                                    var salesOrder = await _salesOrderRepository.GetSalesOrderByIdAsync(salesOrderId);
                                    if (salesOrder != null)
                                    {
                                        success = await SynchronizeSalesOrderAsync(salesOrder);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Sales order not found for retry: {OrderId}", salesOrderId);
                                    }
                                }
                                break;

                            default:
                                _logger.LogWarning("Unknown entity type for retry: {EntityType}", syncStatus.EntityType);
                                break;
                        }

                        results.Add(new SyncResult
                        {
                            EntityType = syncStatus.EntityType,
                            EntityId = syncStatus.EntityId,
                            Success = success,
                            ErrorMessage = success ? string.Empty : $"Failed to retry synchronization for {syncStatus.EntityType} {syncStatus.EntityId}",
                            SyncDate = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error retrying synchronization for {EntityType} {EntityId}",
                            syncStatus.EntityType, syncStatus.EntityId);

                        results.Add(new SyncResult
                        {
                            EntityType = syncStatus.EntityType,
                            EntityId = syncStatus.EntityId,
                            Success = false,
                            ErrorMessage = ex.Message,
                            SyncDate = DateTime.UtcNow
                        });
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying failed synchronizations");
                return new List<SyncResult>();
            }
        }

        public async Task<SyncStatus> GetSyncStatusAsync(string entityType, string entityId)
        {
            if (string.IsNullOrEmpty(entityType))
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (string.IsNullOrEmpty(entityId))
            {
                throw new ArgumentNullException(nameof(entityId));
            }

            try
            {
                _logger.LogInformation("Getting sync status for {EntityType} {EntityId}", entityType, entityId);

                // Get the sync status from the repository
                var syncStatus = await _syncRepository.GetSyncStatusAsync(entityType, entityId);

                if (syncStatus == null)
                {
                    // If no sync status exists, create a new one with "Pending" status
                    syncStatus = new SyncStatus
                    {
                        EntityType = entityType,
                        EntityId = entityId,
                        Status = "Pending",
                        LastSyncDate = null,
                        RetryCount = 0,
                        LastErrorMessage = string.Empty
                    };
                }

                return syncStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync status for {EntityType} {EntityId}", entityType, entityId);

                // Return a default sync status with "Unknown" status
                return new SyncStatus
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Status = "Unknown",
                    LastSyncDate = null,
                    RetryCount = 0,
                    LastErrorMessage = $"Error retrieving sync status: {ex.Message}"
                };
            }
        }
    }
}

