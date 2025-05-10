using GacWmsIntegration.Core.Models;

namespace GacWmsIntegration.Core.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByCodeAsync(string productCode);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(string productCode);
        Task<bool> ProductExistsAsync(string productCode);
        Task<bool> ValidateProductAsync(Product product);
        Task<bool> SyncProductWithWmsAsync(string productCode);
    }
}
