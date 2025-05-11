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
            var customer = new Customer
            {
                CustomerID = 1,
                Name = "Customer 1",
                Address = "Address 1"
            };

            await _dbContext.Customers.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            var updatedCustomer = new Customer
            {
                CustomerID = 1,
                Name = "Updated Customer",
                Address = "Updated Address"
            };

            // Act
            var result = await _customerService.UpdateCustomerAsync(updatedCustomer);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CustomerID);
            Assert.AreEqual("Updated Customer", result.Name);
            Assert.AreEqual("Updated Address", result.Address);

            // Verify the customer was updated in the database
            var savedCustomer = await _dbContext.Customers.FindAsync(1);
            Assert.IsNotNull(savedCustomer);
            Assert.AreEqual("Updated Customer", savedCustomer.Name);
            Assert.AreEqual("Updated Address", savedCustomer.Address);
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
