using GacWmsIntegration.Api.Controllers;
using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GacWmsIntegrationTest.Controllers
{
    [TestClass]
    public class CustomersControllerTests
    {
        private Mock<ICustomerService> _mockCustomerService;
        private Mock<ILogger<CustomersController>> _mockLogger;
        private CustomersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockCustomerService = new Mock<ICustomerService>();
            _mockLogger = new Mock<ILogger<CustomersController>>();
            _controller = new CustomersController(_mockCustomerService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetCustomers_ReturnsOkResult_WithListOfCustomers()
        {
            // Arrange
            var customers = TestDataProvider.GetCustomers();
            _mockCustomerService.Setup(service => service.GetAllCustomersAsync())
                .ReturnsAsync(customers);

            // Act
            var actionResult = await _controller.GetCustomers();
            var result = actionResult.Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsInstanceOfType(result.Value, typeof(List<Customer>));

            var returnedCustomers = result.Value as List<Customer>;
            Assert.AreEqual(customers.Count, returnedCustomers.Count);
            Assert.AreEqual(customers[0].CustomerID, returnedCustomers[0].CustomerID);
            Assert.AreEqual(customers[0].Name, returnedCustomers[0].Name);
        }

        [TestMethod]
        public async Task GetCustomers_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _mockCustomerService.Setup(service => service.GetAllCustomersAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var actionResult = await _controller.GetCustomers();
            var result = actionResult.Result as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
            Assert.AreEqual("Error retrieving customers", result.Value);
        }

        [TestMethod]
        public async Task GetCustomer_ReturnsOkResult_WithCustomer()
        {
            // Arrange
            var customer = TestDataProvider.GetCustomerById(1);
            _mockCustomerService.Setup(service => service.GetCustomerByIdAsync(1))
                .ReturnsAsync(customer);

            // Act
            var actionResult = await _controller.GetCustomer(1);
            var result = actionResult.Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var returnedCustomer = result.Value as Customer;
            Assert.IsNotNull(returnedCustomer);
            Assert.AreEqual(customer.CustomerID, returnedCustomer.CustomerID);
            Assert.AreEqual(customer.Name, returnedCustomer.Name);
        }

        [TestMethod]
        public async Task GetCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            _mockCustomerService.Setup(service => service.GetCustomerByIdAsync(1))
                .ThrowsAsync(new KeyNotFoundException("Customer not found"));

            // Act
            var actionResult = await _controller.GetCustomer(1);
            var result = actionResult.Result as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("Customer not found", result.Value);
        }

        [TestMethod]
        public async Task CreateCustomer_ReturnsCreatedResult_WithCustomer()
        {
            // Arrange
            var customer = TestDataProvider.GetCustomerById(1); // Fetch customer from TestDataProvider
            _mockCustomerService.Setup(service => service.CreateCustomerAsync(It.IsAny<Customer>()))
                .ReturnsAsync(customer);

            // Act
            var actionResult = await _controller.CreateCustomer(customer);
            var result = actionResult.Result as CreatedAtActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.StatusCode);
            Assert.AreEqual("GetCustomer", result.ActionName);

            var returnedCustomer = result.Value as Customer;
            Assert.IsNotNull(returnedCustomer);
            Assert.AreEqual(customer.CustomerID, returnedCustomer.CustomerID);
            Assert.AreEqual(customer.Name, returnedCustomer.Name);
        }

        [TestMethod]
        public async Task CreateCustomer_ReturnsBadRequest_WhenCustomerIsNull()
        {
            // Act
            var actionResult = await _controller.CreateCustomer(null!);
            var result = actionResult.Result as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Customer data is null", result.Value);
        }

        [TestMethod]
        public async Task UpdateCustomer_ReturnsOkResult_WithUpdatedCustomer()
        {
            // Arrange
            var customer = TestDataProvider.GetCustomerById(1); // Fetch customer from TestDataProvider
            _mockCustomerService.Setup(service => service.UpdateCustomerAsync(It.IsAny<Customer>()))
                .ReturnsAsync(customer);

            // Act
            var actionResult = await _controller.UpdateCustomer(1, customer);
            var result = actionResult as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var returnedCustomer = result.Value as Customer;
            Assert.IsNotNull(returnedCustomer);
            Assert.AreEqual(customer.CustomerID, returnedCustomer.CustomerID);
            Assert.AreEqual(customer.Name, returnedCustomer.Name);
        }

        [TestMethod]
        public async Task UpdateCustomer_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var customer = TestDataProvider.GetCustomerById(2); // Fetch customer with ID 2 from TestDataProvider

            // Act
            var actionResult = await _controller.UpdateCustomer(1, customer); // Pass mismatched ID (1)
            var result = actionResult as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Invalid customer data or ID mismatch", result.Value);
        }

        [TestMethod]
        public async Task DeleteCustomer_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            _mockCustomerService.Setup(service => service.DeleteCustomerAsync(1))
                .ReturnsAsync(true);

            // Act
            var actionResult = await _controller.DeleteCustomer(1);
            var result = actionResult as NoContentResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(204, result.StatusCode);
        }

        [TestMethod]
        public async Task DeleteCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            _mockCustomerService.Setup(service => service.DeleteCustomerAsync(1))
                .ReturnsAsync(false);

            // Act
            var actionResult = await _controller.DeleteCustomer(1);
            var result = actionResult as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("Customer with ID 1 not found", result.Value);
        }
    }

    public static class TestDataProvider
    {
        public static List<Customer> GetCustomers()
        {
            return new List<Customer>
        {
            new Customer { CustomerID = 1, Name = "Customer A", Address = "123 Main St" },
            new Customer { CustomerID = 2, Name = "Customer B", Address = "456 Elm St" },
            new Customer { CustomerID = 3, Name = "Customer C", Address = "789 Oak St" },
            new Customer { CustomerID = 4, Name = "Customer D", Address = "101 Maple St" },
            new Customer { CustomerID = 5, Name = "Customer E", Address = "202 Pine St" }
        };
        }

        public static Customer GetCustomerById(int id)
        {
            return GetCustomers().FirstOrDefault(c => c.CustomerID == id);
        }
    }
}

