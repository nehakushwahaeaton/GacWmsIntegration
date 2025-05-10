using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GacWmsIntegration.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IApplicationDbContext dbContext,
            ILogger<ProductService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all products");
                return await _dbContext.Products.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                throw;
            }
        }


        public async Task<Product> GetProductByCodeAsync(string productCode)
        {
            if (string.IsNullOrEmpty(productCode))
            {
                throw new ArgumentException("Product code cannot be null or empty", nameof(productCode));
            }

            try
            {
                _logger.LogInformation("Retrieving product with code: {ProductCode}", productCode);
                var product = await _dbContext.Products.FindAsync(productCode);

                if (product == null)
                {
                    _logger.LogWarning("Product with code: {ProductCode} not found", productCode);
                    return null!;
                }

                return product;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving product with code: {ProductCode}", productCode);
                throw;
            }
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                // Validate product data
                if (!await ValidateProductAsync(product))
                {
                    throw new InvalidOperationException("Product validation failed");
                }

                // Set audit fields
                product.CreatedDate = DateTime.UtcNow;
                product.CreatedBy = Environment.UserName; // Or get from authentication context
                product.ModifiedDate = DateTime.UtcNow;
                product.ModifiedBy = Environment.UserName; // Or get from authentication context

                // Add to database
                await _dbContext.Products.AddAsync(product);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Product created successfully with code: {ProductCode}", product.ProductCode);

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {ProductTitle}", product.Title);
                throw;
            }
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                // Check if product exists
                var existingProduct = await _dbContext.Products.FindAsync(product.ProductCode);
                if (existingProduct == null)
                {
                    _logger.LogWarning("Product with code: {ProductCode} not found for update", product.ProductCode);
                    throw new KeyNotFoundException($"Product with code {product.ProductCode} not found");
                }

                // Validate product data
                if (!await ValidateProductAsync(product))
                {
                    throw new InvalidOperationException("Product validation failed");
                }

                // Update audit fields
                product.CreatedDate = existingProduct.CreatedDate; // Preserve original creation date
                product.CreatedBy = existingProduct.CreatedBy; // Preserve original creator
                product.ModifiedDate = DateTime.UtcNow;
                product.ModifiedBy = Environment.UserName; // Or get from authentication context

                // Update entity properties manually
                existingProduct.Title = product.Title;
                existingProduct.Description = product.Description;
                existingProduct.Dimensions = product.Dimensions;
                existingProduct.ModifiedDate = product.ModifiedDate;
                existingProduct.ModifiedBy = product.ModifiedBy;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Product updated successfully with code: {ProductCode}", product.ProductCode);

                return existingProduct;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error updating product with code: {ProductCode}", product.ProductCode);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(string productCode)
        {
            if (string.IsNullOrEmpty(productCode))
            {
                throw new ArgumentException("Product code cannot be null or empty", nameof(productCode));
            }

            try
            {
                // Check if product exists
                var product = await _dbContext.Products.FindAsync(productCode);
                if (product == null)
                {
                    _logger.LogWarning("Product with code: {ProductCode} not found for deletion", productCode);
                    return false;
                }

                // Check if product has related records
                bool hasRelatedRecords = await _dbContext.PurchaseOrderDetails.AnyAsync(pod => pod.ProductCode == productCode) ||
                                         await _dbContext.SalesOrderDetails.AnyAsync(sod => sod.ProductCode == productCode);

                if (hasRelatedRecords)
                {
                    _logger.LogWarning("Cannot delete product with code: {ProductCode} because it has related records", productCode);
                    throw new InvalidOperationException($"Cannot delete product with code {productCode} because it has related records");
                }

                // Remove from database
                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Product deleted successfully with code: {ProductCode}", productCode);
                return true;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error deleting product with code: {ProductCode}", productCode);
                throw;
            }
        }

        public async Task<bool> ProductExistsAsync(string productCode)
        {
            if (string.IsNullOrEmpty(productCode))
            {
                throw new ArgumentException("Product code cannot be null or empty", nameof(productCode));
            }

            try
            {
                return await _dbContext.Products.AnyAsync(p => p.ProductCode == productCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product exists with code: {ProductCode}", productCode);
                throw;
            }
        }

        public async Task<bool> ValidateProductAsync(Product product)
        {
            if (product == null)
            {
                return false;
            }

            try
            {
                // Basic validation rules
                if (string.IsNullOrWhiteSpace(product.ProductCode))
                {
                    _logger.LogWarning("Product validation failed: Product code is required");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(product.Title))
                {
                    _logger.LogWarning("Product validation failed: Title is required");
                    return false;
                }

                // Product code should be unique (except for the current product during updates)
                var existingProduct = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.ProductCode == product.ProductCode && p != product);

                if (existingProduct != null)
                {
                    _logger.LogWarning("Product validation failed: Duplicate product code {ProductCode}", product.ProductCode);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating product: {ProductTitle}", product.Title);
                throw;
            }
        }

    }
}
