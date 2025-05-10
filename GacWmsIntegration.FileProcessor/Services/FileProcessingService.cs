using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.FileProcessor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GacWmsIntegration.FileProcessor.Services
{
    /// <summary>
    /// Service for processing files according to configured patterns
    /// </summary>
    public class FileProcessingService : IDisposable
    {
        private readonly ILogger<FileProcessingService> _logger;
        private readonly FileProcessingConfig _config;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISalesOrderService _salesOrderService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly Dictionary<string, int> _processingRetries = new Dictionary<string, int>();
        private readonly SemaphoreSlim _processingSemaphore = new SemaphoreSlim(1, 1);
        private bool _disposed = false;

        public FileProcessingService(
            IOptions<FileProcessingConfig> config,
            ILogger<FileProcessingService> logger,
            IPurchaseOrderService purchaseOrderService,
            ISalesOrderService salesOrderService,
            IProductService productService,
            ICustomerService customerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _purchaseOrderService = purchaseOrderService ?? throw new ArgumentNullException(nameof(purchaseOrderService));
            _salesOrderService = salesOrderService ?? throw new ArgumentNullException(nameof(salesOrderService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            // Ensure directories exist
            EnsureDirectoriesExist();
        }

        ///// <summary>
        ///// Process all files matching the configured patterns
        ///// </summary>
        //public async Task ProcessFilesAsync(CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        await _processingSemaphore.WaitAsync(cancellationToken);

        //        _logger.LogInformation("Starting file processing cycle");

        //        foreach (var patternConfig in _config.FilePatterns.Where(p => p.IsEnabled))
        //        {
        //            if (cancellationToken.IsCancellationRequested)
        //            {
        //                _logger.LogInformation("File processing cancelled");
        //                break;
        //            }

        //            try
        //            {
        //                await ProcessFilePatternAsync(patternConfig, cancellationToken);
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(ex, "Error processing file pattern {PatternName}: {ErrorMessage}",
        //                    patternConfig.Name, ex.Message);
        //            }
        //        }

        //        _logger.LogInformation("Completed file processing cycle");
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        _logger.LogInformation("File processing operation was cancelled");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error during file processing: {ErrorMessage}", ex.Message);
        //    }
        //    finally
        //    {
        //        if (_processingSemaphore.CurrentCount == 0)
        //        {
        //            _processingSemaphore.Release();
        //        }
        //    }
        //}

        ///// <summary>
        ///// Process files matching a specific pattern configuration
        ///// </summary>
        //private async Task ProcessFilePatternAsync(FilePatternConfig patternConfig, CancellationToken cancellationToken)
        //{
        //    var inputDir = Path.Combine(_config.BaseDirectory, _config.InputDirectory);
        //    var files = Directory.GetFiles(inputDir, patternConfig.Pattern);

        //    _logger.LogInformation("Found {FileCount} files matching pattern {Pattern}",
        //        files.Length, patternConfig.Pattern);

        //    foreach (var filePath in files)
        //    {
        //        if (cancellationToken.IsCancellationRequested)
        //        {
        //            break;
        //        }

        //        var fileName = Path.GetFileName(filePath);
        //        _logger.LogInformation("Processing file: {FileName}", fileName);

        //        try
        //        {
        //            // Create backup if configured
        //            if (_config.CreateBackup)
        //            {
        //                await CreateBackupAsync(filePath);
        //            }

        //            // Process the file based on its type
        //            var success = await ProcessFileByTypeAsync(filePath, patternConfig);

        //            if (success)
        //            {
        //                _logger.LogInformation("Successfully processed file: {FileName}", fileName);

        //                // Remove from retry tracking if it was there
        //                _processingRetries.Remove(filePath);

        //                // Handle the processed file
        //                await HandleProcessedFileAsync(filePath);
        //            }
        //            else
        //            {
        //                _logger.LogWarning("Failed to process file: {FileName}", fileName);

        //                // Track retry count
        //                if (!_processingRetries.ContainsKey(filePath))
        //                {
        //                    _processingRetries[filePath] = 1;
        //                }
        //                else
        //                {
        //                    _processingRetries[filePath]++;
        //                }

        //                // Check if max retries reached
        //                if (_processingRetries[filePath] > _config.MaxRetries)
        //                {
        //                    _logger.LogError("Max retries reached for file: {FileName}", fileName);
        //                    await MoveToErrorDirectoryAsync(filePath);
        //                    _processingRetries.Remove(filePath);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error processing file {FileName}: {ErrorMessage}",
        //                fileName, ex.Message);

        //            await MoveToErrorDirectoryAsync(filePath);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Process a file based on its type
        ///// </summary>
        //private async Task<bool> ProcessFileByTypeAsync(string filePath, FilePatternConfig patternConfig)
        //{
        //    switch (patternConfig.FileType.ToLowerInvariant())
        //    {
        //        case "purchaseorder":
        //            return await _purchaseOrderService.ProcessPurchaseOrderFileAsync(filePath);

        //        case "salesorder":
        //            return await _salesOrderService.ProcessSalesOrderFileAsync(filePath);

        //        case "product":
        //            return await _productService.ProcessProductFileAsync(filePath);

        //        case "customer":
        //            return await _customerService.ProcessCustomerFileAsync(filePath);

        //        default:
        //            _logger.LogWarning("Unknown file type: {FileType}", patternConfig.FileType);
        //            return false;
        //    }
        //}

        /// <summary>
        /// Create a backup of a file
        /// </summary>
        private async Task CreateBackupAsync(string filePath)
        {
            var backupDir = Path.Combine(_config.BaseDirectory, _config.BackupDirectory);
            var fileName = Path.GetFileName(filePath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
            var backupPath = Path.Combine(backupDir, backupFileName);

            try
            {
                using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var destinationStream = new FileStream(backupPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                _logger.LogInformation("Created backup of file {FileName} at {BackupPath}", fileName, backupPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create backup of file {FileName}: {ErrorMessage}",
                    fileName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Handle a successfully processed file
        /// </summary>
        private async Task HandleProcessedFileAsync(string filePath)
        {
            if (_config.DeleteProcessedFiles)
            {
                try
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted processed file: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete processed file {FilePath}: {ErrorMessage}",
                        filePath, ex.Message);
                }
            }
            else if (_config.MoveProcessedFiles)
            {
                await MoveToProcessedDirectoryAsync(filePath);
            }
        }

        /// <summary>
        /// Move a file to the processed directory
        /// </summary>
        private async Task MoveToProcessedDirectoryAsync(string filePath)
        {
            var processedDir = Path.Combine(_config.BaseDirectory, _config.ProcessedDirectory);
            var fileName = Path.GetFileName(filePath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var processedFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
            var processedPath = Path.Combine(processedDir, processedFileName);

            try
            {
                // Use File.Copy and then delete to avoid issues with locked files
                using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var destinationStream = new FileStream(processedPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                File.Delete(filePath);

                _logger.LogInformation("Moved processed file {FileName} to {ProcessedPath}", fileName, processedPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move processed file {FileName} to processed directory: {ErrorMessage}",
                    fileName, ex.Message);
            }
        }

        /// <summary>
        /// Move a file to the error directory
        /// </summary>
        private async Task MoveToErrorDirectoryAsync(string filePath)
        {
            if (!_config.MoveErrorFiles)
            {
                return;
            }

            var errorDir = Path.Combine(_config.BaseDirectory, _config.ErrorDirectory);
            var fileName = Path.GetFileName(filePath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var errorFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
            var errorPath = Path.Combine(errorDir, errorFileName);

            try
            {
                // Use File.Copy and then delete to avoid issues with locked files
                using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var destinationStream = new FileStream(errorPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                File.Delete(filePath);

                _logger.LogInformation("Moved error file {FileName} to {ErrorPath}", fileName, errorPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move error file {FileName} to error directory: {ErrorMessage}",
                    fileName, ex.Message);
            }
        }
        /// <summary>
        /// Ensure all required directories exist
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            try
            {
                var baseDir = _config.BaseDirectory;

                Directory.CreateDirectory(Path.Combine(baseDir, _config.InputDirectory));
                Directory.CreateDirectory(Path.Combine(baseDir, _config.ProcessedDirectory));
                Directory.CreateDirectory(Path.Combine(baseDir, _config.ErrorDirectory));
                Directory.CreateDirectory(Path.Combine(baseDir, _config.BackupDirectory));

                _logger.LogInformation("Ensured all required directories exist");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create required directories: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _processingSemaphore?.Dispose();
                }

                _disposed = true;
            }
        }
    }

}
