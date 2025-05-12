using GacWmsIntegration.Core.Models;
using GacWmsIntegration.Core.Services;
using GacWmsIntegration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GacWmsIntegration.Core.Tests.Services
{
    [TestClass]
    public class CustomerServiceTests
    {
        private ApplicationDbContext _dbContext = null!;
        private Mock<ILogger<ApplicationDbContext>> _mockDbContextLogger = null!;
        private Mock<ILogger<CustomerService>> _mockServiceLogger = null!;
        private CustomerService _customerService = null!;

        [TestInitialize]
        public void Setup()
        {
            // Create loggers
            _mockDbContextLogger = new Mock<ILogger<ApplicationDbContext>>();
            _mockServiceLogger = new Mock<ILogger<CustomerService>>();

            // Create in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options, _mockDbContextLogger.Object);
            _customerService = new CustomerService(_dbContext, _mockServiceLogger.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [TestMethod]
        public async Task CreateCustomerAsync_AddsCustomerToDatabase()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerID = 1,
                Name = "Customer 1",
                Address = "Address 1"
            };

            // Act
            var result = await _customerService.CreateCustomerAsync(customer);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CustomerID);
            Assert.AreEqual("Customer 1", result.Name);
            Assert.AreEqual("Address 1", result.Address);

            // Verify the customer was added to the database
            var savedCustomer = await _dbContext.Customers.FindAsync(1);
            Assert.IsNotNull(savedCustomer);
            Assert.AreEqual("Customer 1", savedCustomer.Name);
            Assert.AreEqual("Address 1", savedCustomer.Address);
        }

        [TestMethod]
        public async Task GetCustomerByIdAsync_ReturnsCustomer_WhenCustomerExists()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerID = 1,
                Name = "Customer 1",
                Address = "Address 1"
            };

            await _dbContext.Customers.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _customerService.GetCustomerByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CustomerID);
            Assert.AreEqual("Customer 1", result.Name);
        }

        [TestMethod]
        public async Task GetAllCustomersAsync_ReturnsAllCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { CustomerID = 1, Name = "Customer 1", Address = "Address 1" },
                new Customer { CustomerID = 2, Name = "Customer 2", Address = "Address 2" }
            };

            await _dbContext.Customers.AddRangeAsync(customers);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _customerService.GetAllCustomersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(c => c.CustomerID == 1));
            Assert.IsTrue(result.Any(c => c.CustomerID == 2));
        }

        [TestMethod]
        public async Task UpdateCustomerAsync_UpdatesCustomer_WhenCustomerExists()
        {
            // Arrange
            var originalCreationDate = DateTime.UtcNow.AddDays(-10);
            var originalCreator = "OriginalUser";

            var customer = new Customer
            {
                CustomerID = 1,
                Name = "Customer 1",
                Address = "Address 1",
                CreatedDate = originalCreationDate,
                CreatedBy = originalCreator,
                ModifiedDate = originalCreationDate,
                ModifiedBy = originalCreator
            };

            await _dbContext.Customers.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            // Store the exact values from the database for comparison
            var storedCustomer = await _dbContext.Customers.FindAsync(1);
            var storedCreationDate = storedCustomer.CreatedDate;
            var storedCreator = storedCustomer.CreatedBy;

            // Get the customer from the database and update it
            var customerToUpdate = await _dbContext.Customers.FindAsync(1);
            customerToUpdate.Name = "Updated Customer";
            customerToUpdate.Address = "Updated Address";

            // Act
            var result = await _customerService.UpdateCustomerAsync(customerToUpdate);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CustomerID);
            Assert.AreEqual("Updated Customer", result.Name);
            Assert.AreEqual("Updated Address", result.Address);

            // Verify audit fields - compare with the stored values
            Assert.AreEqual(storedCreationDate, result.CreatedDate, "Original creation date should be preserved");
            Assert.AreEqual(storedCreator, result.CreatedBy, "Original creator should be preserved");
            Assert.IsTrue(DateTime.UtcNow.AddMinutes(-1) <= result.ModifiedDate, "Modified date should be updated");
            Assert.AreEqual(Environment.UserName, result.ModifiedBy, "Modified by should be updated");

            // Verify the customer was updated in the database
            var savedCustomer = await _dbContext.Customers.FindAsync(1);
            Assert.IsNotNull(savedCustomer);
            Assert.AreEqual("Updated Customer", savedCustomer.Name);
            Assert.AreEqual("Updated Address", savedCustomer.Address);
            Assert.AreEqual(storedCreationDate, savedCustomer.CreatedDate);
            Assert.AreEqual(storedCreator, savedCustomer.CreatedBy);
            Assert.IsTrue(DateTime.UtcNow.AddMinutes(-1) <= savedCustomer.ModifiedDate);
            Assert.AreEqual(Environment.UserName, savedCustomer.ModifiedBy);
        }


        [TestMethod]
        public async Task UpdateCustomerAsync_ThrowsKeyNotFoundException_WhenCustomerDoesNotExist()
        {
            // Arrange
            var nonExistentCustomer = new Customer
            {
                CustomerID = 999, // ID that doesn't exist
                Name = "Non-existent Customer",
                Address = "Non-existent Address"
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
                async () => await _customerService.UpdateCustomerAsync(nonExistentCustomer));
        }

        [TestMethod]
        public async Task UpdateCustomerAsync_ThrowsInvalidOperationException_WhenValidationFails()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerID = 1,
                Name = "Customer 1",
                Address = "Address 1"
            };

            await _dbContext.Customers.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            // Create a customer that will fail validation
            // You need to know what conditions will cause validation to fail
            // For example, if empty name fails validation:
            var invalidCustomer = new Customer
            {
                CustomerID = 1,
                Name = "", // Empty name that should fail validation
                Address = "Updated Address"
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _customerService.UpdateCustomerAsync(invalidCustomer));
        }

        [TestMethod]
        public async Task UpdateCustomerAsync_ThrowsArgumentNullException_WhenCustomerIsNull()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () => await _customerService.UpdateCustomerAsync(null));
        }


        [TestMethod]
        public async Task DeleteCustomerAsync_DeletesCustomer_WhenCustomerExists()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerID = 1,
                Name = "Customer 1",
                Address = "Address 1"
            };

            await _dbContext.Customers.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _customerService.DeleteCustomerAsync(1);

            // Assert
            Assert.IsTrue(result);

            // Verify the customer was deleted from the database
            var deletedCustomer = await _dbContext.Customers.FindAsync(1);
            Assert.IsNull(deletedCustomer);
        }
    }
}
