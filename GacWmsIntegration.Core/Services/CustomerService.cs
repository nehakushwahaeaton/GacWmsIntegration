using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace GacWmsIntegration.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            IApplicationDbContext dbContext,
            ILogger<CustomerService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all customers");
                return await _dbContext.Customers.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all customers");
                throw;
            }
        }

        public async Task<Customer> GetCustomerByIdAsync(int customerId)
        {
            try
            {
                _logger.LogInformation("Retrieving customer with ID: {CustomerId}", customerId);
                var customer = await _dbContext.Customers.FindAsync(customerId);

                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID: {CustomerId} not found", customerId);
                    return null!;
                }

                return customer;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving customer with ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            try
            {
                // Validate customer data
                if (!await ValidateCustomerAsync(customer))
                {
                    throw new InvalidOperationException("Customer validation failed");
                }

                // Set audit fields
                customer.CreatedDate = DateTime.UtcNow;
                customer.CreatedBy = Environment.UserName; // Or get from authentication context
                customer.ModifiedDate = DateTime.UtcNow;
                customer.ModifiedBy = Environment.UserName; // Or get from authentication context

                // Add to database
                await _dbContext.Customers.AddAsync(customer);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Customer created successfully with ID: {CustomerId}", customer.CustomerID);

                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer: {CustomerName}", customer.Name);
                throw;
            }
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            try
            {
                // Check if customer exists
                var existingCustomer = await _dbContext.Customers.FindAsync(customer.CustomerID);
                if (existingCustomer == null)
                {
                    _logger.LogWarning("Customer with ID: {CustomerId} not found for update", customer.CustomerID);
                    throw new KeyNotFoundException($"Customer with ID {customer.CustomerID} not found");
                }

                // Validate customer data
                if (!await ValidateCustomerAsync(customer))
                {
                    throw new InvalidOperationException("Customer validation failed");
                }

                // Update audit fields
                customer.CreatedDate = existingCustomer.CreatedDate; // Preserve original creation date
                customer.CreatedBy = existingCustomer.CreatedBy; // Preserve original creator
                customer.ModifiedDate = DateTime.UtcNow;
                customer.ModifiedBy = Environment.UserName; // Or get from authentication context

                // Update entity
                _dbContext.Entry(existingCustomer).CurrentValues.SetValues(customer);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Customer updated successfully with ID: {CustomerId}", customer.CustomerID);

                return customer;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", customer.CustomerID);
                throw;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            try
            {
                // Check if customer exists
                var customer = await _dbContext.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID: {CustomerId} not found for deletion", customerId);
                    return false;
                }

                // Check if customer has related records
                bool hasRelatedRecords = await _dbContext.PurchaseOrders.AnyAsync(po => po.CustomerID == customerId) ||
                                         await _dbContext.SalesOrders.AnyAsync(so => so.CustomerID == customerId);

                if (hasRelatedRecords)
                {
                    _logger.LogWarning("Cannot delete customer with ID: {CustomerId} because it has related records", customerId);
                    throw new InvalidOperationException($"Cannot delete customer with ID {customerId} because it has related records");
                }

                // Remove from database
                _dbContext.Customers.Remove(customer);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Customer deleted successfully with ID: {CustomerId}", customerId);
                return true;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            try
            {
                return await _dbContext.Customers.AnyAsync(c => c.CustomerID == customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer exists with ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> ValidateCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                return false;
            }

            try
            {
                // Basic validation rules
                if (string.IsNullOrWhiteSpace(customer.Name))
                {
                    _logger.LogWarning("Customer validation failed: Name is required");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(customer.Address))
                {
                    _logger.LogWarning("Customer validation failed: Address is required");
                    return false;
                }

                // Check for duplicate customer ID (except for the current customer during updates)
                var existingCustomer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.CustomerID == customer.CustomerID && c != customer);

                if (existingCustomer != null)
                {
                    _logger.LogWarning("Customer validation failed: Duplicate customer ID {CustomerId}", customer.CustomerID);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating customer: {CustomerName}", customer.Name);
                throw;
            }
        }
    }
}
