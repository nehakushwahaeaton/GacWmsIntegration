using GacWmsIntegration.Api.Controllers;
using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GacWmsIntegrationTest.Controllers
{
    [TestClass]
    public class ProductsControllerTests
    {
        private Mock<IProductService> _mockProductService;
        private Mock<ILogger<ProductsController>> _mockLogger;
        private ProductsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ILogger<ProductsController>>();
            _controller = new ProductsController(_mockProductService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetProducts_ReturnsOkResult_WithListOfProducts()
        {
            // Arrange
            var products = ProductTestDataProvider.GetProducts();
            _mockProductService.Setup(service => service.GetAllProductsAsync()).ReturnsAsync(products);

            // Act
            var actionResult = await _controller.GetProducts();
            var result = actionResult.Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(products, result.Value);
        }

        [TestMethod]
        public async Task GetProduct_ReturnsOkResult_WithProduct()
        {
            // Arrange
            var product = ProductTestDataProvider.GetProductByCode("P001");
            _mockProductService.Setup(service => service.GetProductByCodeAsync("P001")).ReturnsAsync(product);

            // Act
            var actionResult = await _controller.GetProduct("P001");
            var result = actionResult.Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(product, result.Value);
        }

        [TestMethod]
        public async Task GetProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            _mockProductService.Setup(service => service.GetProductByCodeAsync("XYZ789"))
                .ThrowsAsync(new KeyNotFoundException("Product not found"));

            // Act
            var actionResult = await _controller.GetProduct("XYZ789");
            var result = actionResult.Result as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("Product not found", result.Value);
        }

        [TestMethod]
        public async Task CreateProduct_ReturnsCreatedResult_WithProduct()
        {
            // Arrange
            var product = ProductTestDataProvider.GetProductByCode("P001");
            _mockProductService.Setup(service => service.CreateProductAsync(product)).ReturnsAsync(product);

            // Act
            var actionResult = await _controller.CreateProduct(product);
            var result = actionResult.Result as CreatedAtActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.StatusCode);
            Assert.AreEqual("GetProduct", result.ActionName);
            Assert.AreEqual(product, result.Value);
        }

        [TestMethod]
        public async Task CreateProduct_ReturnsBadRequest_WhenProductIsNull()
        {
            // Act
            var actionResult = await _controller.CreateProduct(null);
            var result = actionResult.Result as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Product data is null", result.Value);
        }

        [TestMethod]
        public async Task UpdateProduct_ReturnsOkResult_WithUpdatedProduct()
        {
            // Arrange
            var product = ProductTestDataProvider.GetProductByCode("P002");
            product.Title = "Updated Product 2";
            _mockProductService.Setup(service => service.UpdateProductAsync(product)).ReturnsAsync(product);

            // Act
            var actionResult = await _controller.UpdateProduct("P002", product);
            var result = actionResult as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(product, result.Value);
        }

        [TestMethod]
        public async Task UpdateProduct_ReturnsBadRequest_WhenCodeMismatch()
        {
            // Arrange
            var product = ProductTestDataProvider.GetProductByCode("P002"); // Fetch product from static table
            product.ProductCode = "DEF456"; // Simulate a mismatch in ProductCode

            // Act
            var actionResult = await _controller.UpdateProduct("ABC123", product);
            var result = actionResult as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Invalid product data or code mismatch", result.Value);
        }

        [TestMethod]
        public async Task DeleteProduct_ReturnsNoContent_WhenProductIsDeleted()
        {
            // Arrange
            _mockProductService.Setup(service => service.DeleteProductAsync("P003")).ReturnsAsync(true);

            // Act
            var actionResult = await _controller.DeleteProduct("P003");
            var result = actionResult as NoContentResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(204, result.StatusCode);
        }

        [TestMethod]
        public async Task DeleteProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            _mockProductService.Setup(service => service.DeleteProductAsync("XYZ789")).ReturnsAsync(false);

            // Act
            var actionResult = await _controller.DeleteProduct("XYZ789");
            var result = actionResult as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("Product with code XYZ789 not found", result.Value);
        }
    }

    public static class ProductTestDataProvider
    {
        public static List<Product> GetProducts()
        {
            return new List<Product>
        {
            new Product
            {
                ProductCode = "P001",
                Title = "Product 1",
                Description = "Description for Product 1",
                Dimensions = "10x10x10",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "SYSTEM_USER",
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = "SYSTEM_USER"
            },
            new Product
            {
                ProductCode = "P002",
                Title = "Product 2",
                Description = "Description for Product 2",
                Dimensions = "20x20x20",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "SYSTEM_USER",
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = "SYSTEM_USER"
            },
            new Product
            {
                ProductCode = "P003",
                Title = "Product 3",
                Description = "Description for Product 3",
                Dimensions = "30x30x30",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "SYSTEM_USER",
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = "SYSTEM_USER"
            }
        };
        }

        public static Product GetProductByCode(string productCode)
        {
            return GetProducts().FirstOrDefault(p => p.ProductCode == productCode);
        }
    }


}
