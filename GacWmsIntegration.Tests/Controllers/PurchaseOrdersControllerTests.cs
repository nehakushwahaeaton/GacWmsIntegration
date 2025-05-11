using GacWmsIntegration.Api.Controllers;
using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GacWmsIntegrationTest.Controllers
{
    [TestClass]
    public class PurchaseOrdersControllerTests
    {
        private Mock<IPurchaseOrderService> _mockPurchaseOrderService;
        private Mock<ILogger<PurchaseOrdersController>> _mockLogger;
        private PurchaseOrdersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockPurchaseOrderService = new Mock<IPurchaseOrderService>();
            _mockLogger = new Mock<ILogger<PurchaseOrdersController>>();
            _controller = new PurchaseOrdersController(_mockPurchaseOrderService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetPurchaseOrders_ReturnsOkResult_WithListOfPurchaseOrders()
        {
            // Arrange
            var purchaseOrders = PurchaseOrderTestDataProvider.GetPurchaseOrders();
            _mockPurchaseOrderService.Setup(service => service.GetAllPurchaseOrdersAsync()).ReturnsAsync(purchaseOrders);

            // Act
            var actionResult = await _controller.GetPurchaseOrders();
            var result = actionResult.Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(purchaseOrders, result.Value);
        }

        [TestMethod]
        public async Task GetPurchaseOrderById_ReturnsOkResult_WithPurchaseOrder()
        {
            // Arrange
            var purchaseOrder = PurchaseOrderTestDataProvider.GetPurchaseOrderById(1);
            _mockPurchaseOrderService.Setup(service => service.GetPurchaseOrderByIdAsync(1)).ReturnsAsync(purchaseOrder);

            // Act
            var actionResult = await _controller.GetPurchaseOrder(1);
            var result = actionResult.Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(purchaseOrder, result.Value);
        }


        [TestMethod]
        public async Task GetPurchaseOrderById_ReturnsNotFound_WhenPurchaseOrderDoesNotExist()
        {
            // Arrange
            _mockPurchaseOrderService.Setup(service => service.GetPurchaseOrderByIdAsync(999)).ReturnsAsync((PurchaseOrder)null!);

            // Act
            var actionResult = await _controller.GetPurchaseOrder(999);
            var result = actionResult.Result as ObjectResult;

            // Assert
            Assert.IsNull   (result.Value);
        }

        [TestMethod]
        public async Task CreatePurchaseOrder_ReturnsCreatedResult_WithPurchaseOrder()
        {
            // Arrange
            var newPurchaseOrder = new PurchaseOrder
            {
                OrderID = 3,
                ProcessingDate = DateTime.UtcNow,
                CustomerID = 3,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "SYSTEM_USER",
                Items = new List<PurchaseOrderDetails>
    {
        new PurchaseOrderDetails { OrderDetailID = 5, OrderID = 3, ProductCode = "P005", Quantity = 50, CreatedDate = DateTime.UtcNow }
    }
            };
            _mockPurchaseOrderService.Setup(service => service.CreatePurchaseOrderAsync(newPurchaseOrder)).ReturnsAsync(newPurchaseOrder);

            // Act
            var actionResult = await _controller.CreatePurchaseOrder(newPurchaseOrder);
            var result = actionResult.Result as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.StatusCode);
            Assert.AreEqual(newPurchaseOrder, result.Value);
        }

        [TestMethod]
        public async Task UpdatePurchaseOrder_ReturnsOkResult_WithUpdatedPurchaseOrder()
        {
            // Arrange
            var updatedPurchaseOrder = PurchaseOrderTestDataProvider.GetPurchaseOrderById(1);
            updatedPurchaseOrder.ProcessingDate = DateTime.UtcNow.AddDays(1);
            _mockPurchaseOrderService.Setup(service => service.UpdatePurchaseOrderAsync(updatedPurchaseOrder)).ReturnsAsync(updatedPurchaseOrder);

            // Act
            var actionResult = await _controller.UpdatePurchaseOrder(1, updatedPurchaseOrder);
            var result = actionResult as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(updatedPurchaseOrder, result.Value);
        }

        [TestMethod]
        public async Task DeletePurchaseOrder_ReturnsNoContent_WhenPurchaseOrderIsDeleted()
        {
            // Arrange
            _mockPurchaseOrderService.Setup(service => service.DeletePurchaseOrderAsync(1)).ReturnsAsync(true);

            // Act
            var actionResult = await _controller.DeletePurchaseOrder(1);
            var result = actionResult as NoContentResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(204, result.StatusCode);
        }

        [TestMethod]
        public async Task DeletePurchaseOrder_ReturnsNotFound_WhenPurchaseOrderDoesNotExist()
        {
            // Arrange
            _mockPurchaseOrderService.Setup(service => service.DeletePurchaseOrderAsync(999)).ReturnsAsync(false);

            // Act
            var actionResult = await _controller.DeletePurchaseOrder(999);
            var result = actionResult as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }
    }

    public static class PurchaseOrderTestDataProvider
    {
        public static List<PurchaseOrder> GetPurchaseOrders()
        {
            return new List<PurchaseOrder>
        {
            new PurchaseOrder
            {
                OrderID = 1,
                ProcessingDate = DateTime.Parse("2023-01-01"),
                CustomerID = 1,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "SYSTEM_USER",
                Items = new List<PurchaseOrderDetails>
                {
                    new PurchaseOrderDetails { OrderDetailID = 1, OrderID = 1, ProductCode = "P001", Quantity = 10, CreatedDate = DateTime.UtcNow },
                    new PurchaseOrderDetails { OrderDetailID = 2, OrderID = 1, ProductCode = "P002", Quantity = 20, CreatedDate = DateTime.UtcNow }
                }
            },
            new PurchaseOrder
            {
                OrderID = 2,
                ProcessingDate = DateTime.Parse("2023-01-02"),
                CustomerID = 2,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "SYSTEM_USER",
                Items = new List<PurchaseOrderDetails>
                {
                    new PurchaseOrderDetails { OrderDetailID = 3, OrderID = 2, ProductCode = "P003", Quantity = 30, CreatedDate = DateTime.UtcNow },
                    new PurchaseOrderDetails { OrderDetailID = 4, OrderID = 2, ProductCode = "P004", Quantity = 40, CreatedDate = DateTime.UtcNow }
                }
            }
        };
        }

        public static PurchaseOrder GetPurchaseOrderById(int orderId)
        {
            return GetPurchaseOrders().FirstOrDefault(po => po.OrderID == orderId);
        }
    }

}
