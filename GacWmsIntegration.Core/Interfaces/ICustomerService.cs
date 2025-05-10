using GacWmsIntegration.Core.Models;

namespace GacWmsIntegration.Core.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer> GetCustomerByIdAsync(int customerId);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int customerId);
        Task<bool> CustomerExistsAsync(int customerId);
        Task<bool> ValidateCustomerAsync(Customer customer);
        Task<bool> SyncCustomerWithWmsAsync(int customerId);
    }
}
