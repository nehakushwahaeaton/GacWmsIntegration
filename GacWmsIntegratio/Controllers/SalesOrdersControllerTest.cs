using GacWmsIntegration.Api.Controllers;
using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GacWmsIntegrationTest.Controllers
{
    [TestClass]
    public class SalesOrdersControllerTest
    {
        private Mock<ISalesOrderService> _mockService;
        private Mock<ILogger<SalesOrdersController>> _mockLogger;
        private SalesOrdersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockService = new Mock<ISalesOrderService>();
            _mockLogger = new Mock<ILogger<SalesOrdersController>>();
            _controller = new SalesOrdersController(_mockService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetSalesOrders_ReturnsOkResult_WithListOfSalesOrders()
        {
            // Arrange
            var salesOrders = SalesOrderTestDataProvider.GetSalesOrders();
            _mockService.Setup(s => s.GetAllSalesOrdersAsync()).ReturnsAsync(salesOrders);

            // Act
            var result = await _controller.GetSalesOrders();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.IsInstanceOfType(okResult.Value, typeof(IEnumerable<SalesOrder>));
            var returnedOrders = (IEnumerable<SalesOrder>)okResult.Value;
            Assert.AreEqual(2, returnedOrders.Count());
        }

        [TestMethod]
        public async Task GetSalesOrder_ReturnsOkResult_WithSalesOrder()
        {
            // Arrange
            var salesOrder = SalesOrderTestDataProvider.GetSingleSalesOrder();
            _mockService.Setup(s => s.GetSalesOrderByIdAsync(3)).ReturnsAsync(salesOrder);

            // Act
            var result = await _controller.GetSalesOrder(3);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.IsInstanceOfType(okResult.Value, typeof(SalesOrder));
            var returnedOrder = (SalesOrder)okResult.Value;
            Assert.AreEqual(3, returnedOrder.OrderID);
        }

        [TestMethod]
        public async Task GetSalesOrder_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            _mockService.Setup(s => s.GetSalesOrderByIdAsync(It.IsAny<int>())).ReturnsAsync((SalesOrder)null!);

            // Act
            var result = await _controller.GetSalesOrder(99);

            // Assert
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public async Task CreateSalesOrder_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var newOrder = SalesOrderTestDataProvider.GetSingleSalesOrder();
            _mockService.Setup(s => s.CreateSalesOrderAsync(It.IsAny<SalesOrder>())).ReturnsAsync(newOrder);

            // Act
            var result = await _controller.CreateSalesOrder(newOrder);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = (CreatedAtActionResult)result.Result;
            Assert.IsInstanceOfType(createdResult.Value, typeof(SalesOrder));
            var returnedOrder = (SalesOrder)createdResult.Value;
            Assert.AreEqual(3, returnedOrder.OrderID);
        }

        [TestMethod]
        public async Task CreateSalesOrder_ReturnsBadRequest_WhenOrderIsNull()
        {
            // Act
            var result = await _controller.CreateSalesOrder(null);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task UpdateSalesOrder_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var updatedOrder = SalesOrderTestDataProvider.GetSingleSalesOrder();
            _mockService.Setup(s => s.UpdateSalesOrderAsync(It.IsAny<SalesOrder>())).ReturnsAsync(updatedOrder);

            // Act
            var result = await _controller.UpdateSalesOrder(3, updatedOrder);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsInstanceOfType(okResult.Value, typeof(SalesOrder));
            var returnedOrder = (SalesOrder)okResult.Value;
            Assert.AreEqual(3, returnedOrder.OrderID);
        }

        [TestMethod]
        public async Task UpdateSalesOrder_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var updatedOrder = SalesOrderTestDataProvider.GetSingleSalesOrder();

            // Act
            var result = await _controller.UpdateSalesOrder(99, updatedOrder);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task DeleteSalesOrder_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteSalesOrderAsync(3)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteSalesOrder(3);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task DeleteSalesOrder_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteSalesOrderAsync(99)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteSalesOrder(99);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task GetOrderItems_ReturnsOkResult_WithListOfItems()
        {
            // Arrange
            var orderItems = SalesOrderTestDataProvider.GetSingleSalesOrder().Items.ToList();
            _mockService.Setup(s => s.GetOrderItemsAsync(3)).ReturnsAsync(orderItems);

            // Act
            var result = await _controller.GetOrderItems(3);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.IsInstanceOfType(okResult.Value, typeof(IEnumerable<SalesOrderDetails>));
            var returnedItems = (IEnumerable<SalesOrderDetails>)okResult.Value;
            Assert.AreEqual(1, returnedItems.Count());
        }

        [TestMethod]
        public async Task GetOrderItems_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            _mockService.Setup(s => s.GetOrderItemsAsync(It.IsAny<int>())).ReturnsAsync((List<SalesOrderDetails>)null);

            // Act
            var result = await _controller.GetOrderItems(99);

            // Assert
            Assert.IsNull(result.Value);
        }
    }

    public static class SalesOrderTestDataProvider
    {
        public static List<SalesOrder> GetSalesOrders()
        {
            return new List<SalesOrder>
            {
                new SalesOrder
                {
                    OrderID = 1,
                    ProcessingDate = new DateTime(2023, 2, 1),
                    CustomerID = 1,
                    ShipmentAddress = "123 Main St",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "SYSTEM_USER",
                    Items = new List<SalesOrderDetails>
                    {
                        new SalesOrderDetails { OrderDetailID = 1, OrderID = 1, ProductCode = "P001", Quantity = 15, CreatedDate = DateTime.UtcNow },
                        new SalesOrderDetails { OrderDetailID = 2, OrderID = 1, ProductCode = "P002", Quantity = 25, CreatedDate = DateTime.UtcNow }
                    }
                },
                new SalesOrder
                {
                    OrderID = 2,
                    ProcessingDate = new DateTime(2023, 2, 2),
                    CustomerID = 2,
                    ShipmentAddress = "456 Elm St",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "SYSTEM_USER",
                    Items = new List<SalesOrderDetails>
                    {
                        new SalesOrderDetails { OrderDetailID = 3, OrderID = 2, ProductCode = "P003", Quantity = 35, CreatedDate = DateTime.UtcNow },
                        new SalesOrderDetails { OrderDetailID = 4, OrderID = 2, ProductCode = "P004", Quantity = 45, CreatedDate = DateTime.UtcNow }
                    }
                }
            };
        }

        public static SalesOrder GetSingleSalesOrder()
        {
            return new SalesOrder
            {
                OrderID = 3,
                ProcessingDate = new DateTime(2023, 2, 3),
                CustomerID = 3,
                ShipmentAddress = "789 Oak St",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "SYSTEM_USER",
                Items = new List<SalesOrderDetails>
                {
                    new SalesOrderDetails { OrderDetailID = 5, OrderID = 3, ProductCode = "P005", Quantity = 55, CreatedDate = DateTime.UtcNow }
                }
            };
        }
    }
}
