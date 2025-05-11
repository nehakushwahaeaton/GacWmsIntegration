using GacWmsIntegration.Core.Models;
using GacWmsIntegration.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace GacWmsIntegration.Core.Tests.Services
{
    [TestClass]
    public class ProductServiceTests
    {
        private Mock<IApplicationDbContext> _mockDbContext;
        private Mock<ILogger<ProductService>> _mockLogger;
        private ProductService _productService;
        private Mock<DbSet<Product>> _mockProductDbSet;
        private List<Product> _products;

        [TestInitialize]
        public void Setup()
        {
            // Initialize test data
            _products = new List<Product>
        {
            new Product
            {
                ProductCode = "P001",
                Title = "Product 1",
                Description = "Description 1",
                Dimensions = "10x10x10",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "TestUser",
                ModifiedDate = DateTime.UtcNow.AddDays(-5),
                ModifiedBy = "TestUser"
            },
            new Product
            {
                ProductCode = "P002",
                Title = "Product 2",
                Description = "Description 2",
                Dimensions = "20x20x20",
                CreatedDate = DateTime.UtcNow.AddDays(-8),
                CreatedBy = "TestUser",
                ModifiedDate = DateTime.UtcNow.AddDays(-3),
                ModifiedBy = "TestUser"
            }
        };

            // Setup mock DbSet
            _mockProductDbSet = CreateMockDbSet(_products);

            // Setup mock DbContext
            _mockDbContext = new Mock<IApplicationDbContext>();
            _mockDbContext.Setup(db => db.Products).Returns(_mockProductDbSet.Object);
            _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Setup logger
            _mockLogger = new Mock<ILogger<ProductService>>();

            // Create ProductService instance
            _productService = new ProductService(_mockDbContext.Object, _mockLogger.Object);
        }
        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryableData = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            // Setup IQueryable
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());

            // Setup async operations
            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryableData.Provider));

            // Setup FindAsync
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .Returns((object[] keys) =>
                {
                    if (typeof(T) == typeof(Product) && keys.Length > 0)
                    {
                        var productCode = keys[0].ToString();
                        var product = data.FirstOrDefault(d =>
                            ((Product)(object)d).ProductCode == productCode) as T;
                        return new ValueTask<T>(product);
                    }
                    return new ValueTask<T>((T)null);
                });

            // Setup AddAsync
            mockSet.Setup(m => m.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
                .Callback<T, CancellationToken>((entity, _) => data.Add(entity))
                .ReturnsAsync((T entity, CancellationToken _) => {
                    var mockEntry = new Mock<EntityEntry<T>>();
                    mockEntry.Setup(e => e.Entity).Returns(entity);
                    return mockEntry.Object;
                });

            // Setup Remove
            mockSet.Setup(m => m.Remove(It.IsAny<T>()))
                .Callback<T>(entity => data.Remove(entity));

            return mockSet;
        }

        public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider)
                    .GetMethod(
                        name: nameof(IQueryProvider.Execute),
                        genericParameterCount: 1,
                        types: new[] { typeof(Expression) })
                    .MakeGenericMethod(resultType)
                    .Invoke(this, new[] { expression });

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new[] { executionResult });
            }
        }

        public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable)
            { }

            public TestAsyncEnumerable(Expression expression)
                : base(expression)
            { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            //IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this.Provider);
        }


        [TestMethod]
        public async Task GetAllProductsAsync_ReturnsAllProducts()
        {
            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(p => p.ProductCode == "P001"));
            Assert.IsTrue(result.Any(p => p.ProductCode == "P002"));
        }

        [TestMethod]
        public async Task GetProductByCodeAsync_ReturnsProduct_WhenProductExists()
        {
            // Act
            var result = await _productService.GetProductByCodeAsync("P001");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("P001", result.ProductCode);
            Assert.AreEqual("Product 1", result.Title);
        }

        [TestMethod]
        public async Task GetProductByCodeAsync_ReturnsNull_WhenProductDoesNotExist()
        {
            // Act
            var result = await _productService.GetProductByCodeAsync("NonExistentCode");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetProductByCodeAsync_ThrowsArgumentException_WhenProductCodeIsNull()
        {
            // Act
            await _productService.GetProductByCodeAsync(null);
        }

        //[TestMethod]
        public async Task CreateProductAsync_AddsProductToDatabase()
        {
            // Arrange
            var newProduct = new Product
            {
                ProductCode = "P003",
                Title = "Product 3",
                Description = "Description 3",
                Dimensions = "30x30x30"
            };

            // Setup the AddAsync method to add the product to the list instead of returning an EntityEntry
            var products = new List<Product>();
            _mockProductDbSet.Setup(m => m.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .Callback<Product, CancellationToken>((p, _) => products.Add(p))
                .ReturnsAsync((Product product, CancellationToken _) =>
                {
                    var mockEntityEntry = new Mock<EntityEntry<Product>>();
                    mockEntityEntry.Setup(e => e.Entity).Returns(product);
                    return mockEntityEntry.Object;
                });

            // Act
            var result = await _productService.CreateProductAsync(newProduct);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("P003", result.ProductCode);
            Assert.AreEqual("Product 3", result.Title);
            Assert.AreEqual(1, products.Count);
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateProductAsync_ThrowsArgumentNullException_WhenProductIsNull()
        {
            // Act
            await _productService.CreateProductAsync(null);
        }

        //[TestMethod]
        public async Task UpdateProductAsync_UpdatesProduct_WhenProductExists()
        {
            // Arrange
            var updatedProduct = new Product
            {
                ProductCode = "P001",
                Title = "Updated Product 1",
                Description = "Updated Description 1",
                Dimensions = "15x15x15"
            };

            // Act
            var result = await _productService.UpdateProductAsync(updatedProduct);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("P001", result.ProductCode);
            Assert.AreEqual("Updated Product 1", result.Title);
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task UpdateProductAsync_ThrowsKeyNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var nonExistentProduct = new Product
            {
                ProductCode = "NonExistentCode",
                Title = "Non-existent Product"
            };

            // Act
            await _productService.UpdateProductAsync(nonExistentProduct);
        }

        //[TestMethod]
        public async Task DeleteProductAsync_DeletesProduct_WhenProductExists()
        {
            // Act
            var result = await _productService.DeleteProductAsync("P001");

            // Assert
            Assert.IsTrue(result);
            _mockProductDbSet.Verify(m => m.Remove(It.IsAny<Product>()), Times.Once);
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteProductAsync_ReturnsFalse_WhenProductDoesNotExist()
        {
            // Act
            var result = await _productService.DeleteProductAsync("NonExistentCode");

            // Assert
            Assert.IsFalse(result);
        }
    }

}