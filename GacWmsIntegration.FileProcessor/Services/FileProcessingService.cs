using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using GacWmsIntegration.FileProcessor.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Retry;
using Polly;
using System.Diagnostics;
using System.Xml.Linq;
using GacWmsIntegration.FileProcessor.Interfaces;

namespace GacWmsIntegration.FileProcessor.Services
{
    /// <summary>
    /// Service for processing files according to configured patterns
    /// </summary>
    public class FileProcessingService
    {
        private readonly ILogger<FileProcessingService> _logger;
        private readonly FileProcessingConfig _config;
        private readonly IXmlParserService _xmlParser;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISalesOrderService _salesOrderService;
        private readonly Dictionary<FileType, AsyncRetryPolicy> _retryPolicies = new();

        public FileProcessingService(
            ILogger<FileProcessingService> logger,
            IOptions<FileProcessingConfig> config,
            IXmlParserService xmlParser,
            ICustomerService customerService,
            IProductService productService,
            IPurchaseOrderService purchaseOrderService,
            ISalesOrderService salesOrderService)
        {
            _logger = logger;
            _config = config.Value;
            _xmlParser = xmlParser;
            _customerService = customerService;
            _productService = productService;
            _purchaseOrderService = purchaseOrderService;
            _salesOrderService = salesOrderService;

            // Initialize retry policies for each file type
            foreach (var fileWatcher in _config.FileWatchers)
            {
                _retryPolicies[fileWatcher.FileType] = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(
                        fileWatcher.MaxRetryAttempts,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            _logger.LogWarning(exception,
                                "Error processing file {FileName}. Retry attempt {RetryCount} after {RetryTimeSpan}s",
                                context["fileName"], retryCount, timeSpan.TotalSeconds);
                        });
            }
        }


        public async Task ProcessFilesAsync(FileWatcherConfig watcherConfig, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting file processing for {WatcherName}", watcherConfig.Name);

                var directory = new DirectoryInfo(watcherConfig.DirectoryPath);
                if (!directory.Exists)
                {
                    _logger.LogWarning("Directory {DirectoryPath} does not exist", watcherConfig.DirectoryPath);
                    return;
                }

                var files = directory.GetFiles(watcherConfig.FilePattern);
                _logger.LogInformation("Found {FileCount} files to process", files.Length);

                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var context = new Context
                    {
                        ["fileName"] = file.Name,
                        ["fileType"] = watcherConfig.FileType
                    };

                    await _retryPolicies[watcherConfig.FileType].ExecuteAsync(async (ctx, ct) =>
                    {
                        await ProcessFileAsync(file.FullName, watcherConfig.FileType);

                        // Archive the file if configured
                        if (watcherConfig.ArchiveProcessedFiles && !string.IsNullOrEmpty(watcherConfig.ArchivePath))
                        {
                            ArchiveFile(file, watcherConfig.ArchivePath);
                        }
                        else
                        {
                            // Delete the file if not archiving
                            file.Delete();
                            _logger.LogInformation("Deleted processed file {FileName}", file.Name);
                        }
                    }, context, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing files for {WatcherName}", watcherConfig.Name);
            }
        }

        /// <summary>
        /// Process all files from all configured watchers
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ProcessFilesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting file processing for all watchers");

            if (_config.FileWatchers == null || _config.FileWatchers.Length == 0)
            {
                _logger.LogWarning("No file watchers configured. Nothing to process.");
                return;
            }

            foreach (var watcherConfig in _config.FileWatchers)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await ProcessFilesAsync(watcherConfig, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing files for watcher {WatcherName}", watcherConfig.Name);
                }
            }

            _logger.LogInformation("Completed file processing for all watchers");
        }


        private async Task ProcessFileAsync(string filePath, FileType fileType)
        {
            _logger.LogInformation("Processing file {FilePath} of type {FileType}", filePath, fileType);

            switch (fileType)
            {
                case FileType.Customer:
                    var customers = _xmlParser.ParseCustomers(filePath);
                    foreach (var customer in customers)
                    {
                        var existingCustomer = await _customerService.GetCustomerByIdAsync(customer.CustomerID);
                        if (existingCustomer != null)
                        {
                            await _customerService.UpdateCustomerAsync(customer);
                        }
                        else
                        {
                            _logger.LogWarning("Customer with ID: {CustomerId} not found. Creating new customer.", customer.CustomerID);
                            await _customerService.CreateCustomerAsync(customer);
                        }

                    }
                    _logger.LogInformation("Processed {Count} customers from {FilePath}", customers.Count, filePath);
                    break;

                case FileType.Product:
                    var products = _xmlParser.ParseProducts(filePath);
                    foreach (var product in products)
                    {
                        var existingProduct = await _productService.GetProductByCodeAsync(product.ProductCode);
                        if (existingProduct != null)
                        {
                            await _productService.UpdateProductAsync(product);
                        }
                        else
                        {
                            await _productService.CreateProductAsync(product);
                        }
                    }
                    _logger.LogInformation("Processed {Count} products from {FilePath}", products.Count, filePath);
                    break;

                case FileType.PurchaseOrder:
                    var purchaseOrders = _xmlParser.ParsePurchaseOrders(filePath);
                    foreach (var order in purchaseOrders)
                    {
                        var existingOrder = await _purchaseOrderService.GetPurchaseOrderByIdAsync(order.OrderID);
                        if (existingOrder != null)
                        {
                            await _purchaseOrderService.UpdatePurchaseOrderAsync(order);
                        }
                        else
                        {
                            await _purchaseOrderService.CreatePurchaseOrderAsync(order);
                        }
                        await _purchaseOrderService.CreatePurchaseOrderAsync(order);
                    }
                    _logger.LogInformation("Processed {Count} purchase orders from {FilePath}", purchaseOrders.Count, filePath);
                    break;

                case FileType.SalesOrder:
                    var salesOrders = _xmlParser.ParseSalesOrders(filePath);
                    foreach (var order in salesOrders)
                    {
                        var existingOrder = await _salesOrderService.GetOrderItemsAsync(order.OrderID);
                        if (existingOrder != null)
                        {
                            await _salesOrderService.UpdateSalesOrderAsync(order);
                        }
                        else
                        {
                            await _salesOrderService.CreateSalesOrderAsync(order);
                        }
                    }
                    _logger.LogInformation("Processed {Count} sales orders from {FilePath}", salesOrders.Count, filePath);
                    break;

                default:
                    _logger.LogWarning("Unknown file type {FileType} for {FilePath}", fileType, filePath);
                    break;
            }
        }
        private void ArchiveFile(FileInfo file, string archivePath)
        {
            try
            {
                Directory.CreateDirectory(archivePath);

                var archiveFilePath = Path.Combine(
                    archivePath,
                    $"{Path.GetFileNameWithoutExtension(file.Name)}_{DateTime.Now:yyyyMMdd_HHmmss}{file.Extension}");

                file.MoveTo(archiveFilePath);
                _logger.LogInformation("Archived file {FileName} to {ArchiveFilePath}", file.Name, archiveFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving file {FileName} to {ArchivePath}", file.Name, archivePath);
                // Still try to delete the original file to prevent reprocessing
                try
                {
                    file.Delete();
                    _logger.LogInformation("Deleted file {FileName} after failed archive attempt", file.Name);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogError(deleteEx, "Failed to delete file {FileName} after failed archive attempt", file.Name);
                }
            }
        }
    }
}

