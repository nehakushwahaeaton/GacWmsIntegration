using GacWmsIntegration.Core.Models;
using GacWmsIntegration.Core.Services;
using GacWmsIntegration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GacWmsIntegration.Core.Tests.Services
{
    [TestClass]
    public class PurchaseOrderServiceTests
    {
        private Mock<IApplicationDbContext> _mockDbContext;
        private Mock<ILogger<PurchaseOrderService>> _mockLogger;
        private PurchaseOrderService _service;
        private Mock<DbSet<PurchaseOrder>> _mockPurchaseOrders;
        private Mock<DbSet<PurchaseOrderDetails>> _mockPurchaseOrderDetails;
        private Mock<DbSet<Customer>> _mockCustomers;
        private Mock<DbSet<Product>> _mockProducts;
        public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }


        [TestInitialize]
        public void Setup()
        {
            _mockDbContext = new Mock<IApplicationDbContext>();
            _mockLogger = new Mock<ILogger<PurchaseOrderService>>();
            _mockPurchaseOrders = new Mock<DbSet<PurchaseOrder>>();
            _mockPurchaseOrderDetails = new Mock<DbSet<PurchaseOrderDetails>>();
            _mockCustomers = new Mock<DbSet<Customer>>();
            _mockProducts = new Mock<DbSet<Product>>();

            _service = new PurchaseOrderService(_mockDbContext.Object, _mockLogger.Object);
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(data.Add);
            mockSet.Setup(m => m.AddAsync(It.IsAny<T>(), default)).ReturnsAsync((T entity, CancellationToken token) =>
            {
                data.Add(entity);
                return null!;
            });
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(item => data.Remove(item));

            return mockSet;
        }

        [TestMethod]
        public async Task GetAllPurchaseOrdersAsync_ReturnsAllOrders()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationDbContext>>();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using var context = new ApplicationDbContext(options, mockLogger.Object);
            context.PurchaseOrders.AddRange(
                new PurchaseOrder { OrderID = 1, Customer = new Customer { Name = "Customer1" } },
                new PurchaseOrder { OrderID = 2, Customer = new Customer { Name = "Customer2" } }
            );
            await context.SaveChangesAsync();

            var service = new PurchaseOrderService(context, _mockLogger.Object);

            // Act
            var result = await service.GetAllPurchaseOrdersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }


        //[TestMethod]
        public async Task GetPurchaseOrderByIdAsync_WithValidId_ReturnsPurchaseOrder()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationDbContext>>();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using var context = new ApplicationDbContext(options, mockLogger.Object);

            // Add related data
            var customer = new Customer { CustomerID = 101, Name = "Customer1" };
            var product = new Product { ProductCode = "201", Title = "Product1" };
            var item = new PurchaseOrderDetails { OrderID = 301, Product = product, Quantity = 2 };
            var testOrder = new PurchaseOrder
            {
                OrderID = 1,
                CustomerID = 101,
                Customer = customer,
                Items = new List<PurchaseOrderDetails> { item }
            };

            context.PurchaseOrders.Add(testOrder);
            await context.SaveChangesAsync();

            var service = new PurchaseOrderService(context, _mockLogger.Object);

            // Act
            var result = await service.GetPurchaseOrderByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.OrderID);
            Assert.AreEqual(101, result.CustomerID);
            Assert.IsNotNull(result.Customer);
            Assert.AreEqual("Customer1", result.Customer.Name);
            Assert.IsNotNull(result.Items);
            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual("201", result.Items.First().Product.ProductCode);
            Assert.AreEqual("Product1", result.Items.First().Product.Title);
        }


        //[TestMethod]
        public async Task GetPurchaseOrderByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationDbContext>>();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using var context = new ApplicationDbContext(options, mockLogger.Object);

            // Add a valid order to the database
            var testOrder = new PurchaseOrder { OrderID = 1, CustomerID = 101 };
            context.PurchaseOrders.Add(testOrder);
            await context.SaveChangesAsync();

            var service = new PurchaseOrderService(context, _mockLogger.Object);

            // Act
            var result = await service.GetPurchaseOrderByIdAsync(999); // Invalid ID

            // Assert
            Assert.IsNull(result);
        }

        //[TestMethod]
        public async Task GetPurchaseOrdersByCustomerAsync_WithValidCustomerId_ReturnsPurchaseOrders()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationDbContext>>();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using var context = new ApplicationDbContext(options, mockLogger.Object);

            // Add related data
            var customer = new Customer { CustomerID = 101, Name = "Customer1" };
            var orders = new List<PurchaseOrder>
    {
        new PurchaseOrder { OrderID = 1, CustomerID = 101, Customer = customer },
        new PurchaseOrder { OrderID = 2, CustomerID = 101, Customer = customer },
        new PurchaseOrder { OrderID = 3, CustomerID = 102 } // Different customer
    };

            context.Customers.Add(customer);
            context.PurchaseOrders.AddRange(orders);
            await context.SaveChangesAsync();

            var service = new PurchaseOrderService(context, _mockLogger.Object);

            // Act
            var result = await service.GetPurchaseOrdersByCustomerAsync(101);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(po => po.CustomerID == 101));
        }

        //[TestMethod]
        public async Task GetPurchaseOrdersByCustomerAsync_WithInvalidCustomerId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationDbContext>>();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using var context = new ApplicationDbContext(options, mockLogger.Object);

            // Add a valid customer and purchase orders to the database
            var validCustomer = new Customer { CustomerID = 101, Name = "Valid Customer" };
            context.Customers.Add(validCustomer);
            context.PurchaseOrders.Add(new PurchaseOrder { OrderID = 1, CustomerID = 101, Customer = validCustomer });
            await context.SaveChangesAsync();

            var service = new PurchaseOrderService(context, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
            {
                await service.GetPurchaseOrdersByCustomerAsync(999); // Invalid CustomerID
            });
        }

        //[TestMethod]
        //public async Task CreatePurchaseOrderAsync_WithValidOrder_CreatesAndReturnsPurchaseOrder()
        //{
        //    // Arrange
        //    int customerId = 101;
        //    string productCode = "PROD001";

        //    var testCustomers = new List<Customer>
        //    {
        //        new Customer { CustomerID = customerId, Name = "Test Customer" }
        //    };

        //    var customerQueryable = testCustomers.AsQueryable();
        //    _mockCustomers.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customerQueryable.Provider);
        //    _mockCustomers.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customerQueryable.Expression);
        //    _mockCustomers.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customerQueryable.ElementType);
        //    _mockCustomers.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(() => customerQueryable.GetEnumerator());

        //    var testProducts = new List<Product>
        //    {
        //        new Product { ProductCode = productCode, Description = "Test Product" }
        //    };

        //    var productQueryable = testProducts.AsQueryable();
        //    _mockProducts.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(productQueryable.Provider);
        //    _mockProducts.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(productQueryable.Expression);
        //    _mockProducts.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(productQueryable.ElementType);
        //    _mockProducts.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(() => productQueryable.GetEnumerator());

        //    var purchaseOrder = new PurchaseOrder
        //    {
        //        CustomerID = customerId,
        //        Items = new List<PurchaseOrderDetails>
        //        {
        //            new PurchaseOrderDetails { ProductCode = productCode, Quantity = 5 }
        //        }
        //    };

        //    _mockDbContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        //    // Act
        //    var result = await _service.CreatePurchaseOrderAsync(purchaseOrder);

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(customerId, result.CustomerID);
        //    _mockDbContext.Verify(c => c.PurchaseOrders.AddAsync(It.IsAny<PurchaseOrder>(), default), Times.Once);
        //    _mockDbContext.Verify(c => c.SaveChangesAsync(), Times.Once);
        //}

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatePurchaseOrderAsync_WithNullOrder_ThrowsArgumentNullException()
        {
            // Act
            await _service.CreatePurchaseOrderAsync(null);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreatePurchaseOrderAsync_WithInvalidCustomer_ThrowsInvalidOperationException()
        {
            // Arrange
            int invalidCustomerId = 999;
            string productCode = "PROD001";

            var testCustomers = new List<Customer>
        {
            new Customer { CustomerID = 101, Name = "Test Customer" }
        };

                var customerQueryable = testCustomers.AsQueryable();
                _mockCustomers = CreateMockDbSet(testCustomers);
                _mockDbContext.Setup(c => c.Customers).Returns(_mockCustomers.Object);

                var testProducts = new List<Product>
        {
            new Product { ProductCode = productCode, Description = "Test Product" }
        };

                var productQueryable = testProducts.AsQueryable();
                _mockProducts = CreateMockDbSet(testProducts);
                _mockDbContext.Setup(c => c.Products).Returns(_mockProducts.Object);

                var purchaseOrder = new PurchaseOrder
                {
                    CustomerID = invalidCustomerId,
                    Items = new List<PurchaseOrderDetails>
            {
                new PurchaseOrderDetails { ProductCode = productCode, Quantity = 5 }
            }
                };

                // Act
                await _service.CreatePurchaseOrderAsync(purchaseOrder);

                // Assert is handled by ExpectedException attribute
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreatePurchaseOrderAsync_WithNoItems_ThrowsInvalidOperationException()
        {
            // Arrange
            int customerId = 101;

            var testCustomers = new List<Customer>
    {
        new Customer { CustomerID = customerId, Name = "Test Customer" }
    };

            _mockCustomers = CreateMockDbSet(testCustomers);
            _mockDbContext.Setup(c => c.Customers).Returns(_mockCustomers.Object);

            var purchaseOrder = new PurchaseOrder
            {
                CustomerID = customerId,
                Items = new List<PurchaseOrderDetails>() // Empty items list
            };

            // Act
            await _service.CreatePurchaseOrderAsync(purchaseOrder);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreatePurchaseOrderAsync_WithInvalidProduct_ThrowsInvalidOperationException()
        {
            // Arrange
            int customerId = 101;
            string invalidProductCode = "INVALID001";

            var testCustomers = new List<Customer>
    {
        new Customer { CustomerID = customerId, Name = "Test Customer" }
    };

            _mockCustomers = CreateMockDbSet(testCustomers);
            _mockDbContext.Setup(c => c.Customers).Returns(_mockCustomers.Object);

            var testProducts = new List<Product>
    {
        new Product { ProductCode = "PROD001", Description = "Valid Product" }
    };

            _mockProducts = CreateMockDbSet(testProducts);
            _mockDbContext.Setup(c => c.Products).Returns(_mockProducts.Object);

            var purchaseOrder = new PurchaseOrder
            {
                CustomerID = customerId,
                Items = new List<PurchaseOrderDetails>
        {
            new PurchaseOrderDetails { ProductCode = invalidProductCode, Quantity = 5 }
        }
            };

            // Act
            await _service.CreatePurchaseOrderAsync(purchaseOrder);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreatePurchaseOrderAsync_WithInvalidQuantity_ThrowsInvalidOperationException()
        {
            // Arrange
            int customerId = 101;
            string productCode = "PROD001";

            var testCustomers = new List<Customer>
    {
        new Customer { CustomerID = customerId, Name = "Test Customer" }
    };

            _mockCustomers = CreateMockDbSet(testCustomers);
            _mockDbContext.Setup(c => c.Customers).Returns(_mockCustomers.Object);

            var testProducts = new List<Product>
    {
        new Product { ProductCode = productCode, Description = "Test Product" }
    };

            _mockProducts = CreateMockDbSet(testProducts);
            _mockDbContext.Setup(c => c.Products).Returns(_mockProducts.Object);

            var purchaseOrder = new PurchaseOrder
            {
                CustomerID = customerId,
                Items = new List<PurchaseOrderDetails>
        {
            new PurchaseOrderDetails { ProductCode = productCode, Quantity = 0 } // Invalid quantity
        }
            };

            // Act
            await _service.CreatePurchaseOrderAsync(purchaseOrder);

            // Assert is handled by ExpectedException attribute
        }

        //[TestMethod]
        //public async Task UpdatePurchaseOrderAsync_WithValidOrder_UpdatesAndReturnsPurchaseOrder()
        //{
        //    // Arrange
        //    int orderId = 1;
        //    int customerId = 101;

        //    var existingOrder = new PurchaseOrder
        //    {
        //        OrderID = orderId,
        //        CustomerID = 102, // Different customer ID
        //        Items = new List<PurchaseOrderDetails>(),
        //        CreatedDate = DateTime.UtcNow.AddDays(-1),
        //        CreatedBy = "TestUser"
        //    };

        //    var testOrders = new List<PurchaseOrder> { existingOrder };
        //    var orderQueryable = testOrders.AsQueryable();

        //    _mockPurchaseOrders.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(orderQueryable.Provider);
        //    _mockPurchaseOrders.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(orderQueryable.Expression);
        //    _mockPurchaseOrders.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(orderQueryable.ElementType);
        //    _mockPurchaseOrders.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(() => orderQueryable.GetEnumerator());

        //    var testCustomers = new List<Customer>
        //    {
        //        new Customer { CustomerID = customerId, Name = "Test Customer" }
        //    };

        //    var customerQueryable = testCustomers.AsQueryable();
        //    _mockCustomers.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customerQueryable.Provider);
        //    _mockCustomers.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customerQueryable.Expression);
        //    _mockCustomers.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customerQueryable.ElementType);
        //    _mockCustomers.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(() => customerQueryable.GetEnumerator());

        //    var updatedOrder = new PurchaseOrder
        //    {
        //        OrderID = orderId,
        //        CustomerID = customerId,
        //        ProcessingDate = DateTime.UtcNow,
        //        Items = new List<PurchaseOrderDetails>
        //        {
        //            new PurchaseOrderDetails { ProductCode = "PROD001", Quantity = 5 }
        //        }
        //    };

        //    _mockDbContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        //    // Setup product validation
        //    var testProducts = new List<Product>
        //    {
        //        new Product { ProductCode = "PROD001", Description = "Test Product" }
        //    };

        //    var productQueryable = testProducts.AsQueryable();
        //    _mockProducts.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(productQueryable.Provider);
        //    _mockProducts.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(productQueryable.Expression);
        //    _mockProducts.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(productQueryable.ElementType);
        //    _mockProducts.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(() => productQueryable.GetEnumerator());

        //    // Act
        //    var result = await _service.UpdatePurchaseOrderAsync(updatedOrder);

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(orderId, result.OrderID);
        //    Assert.AreEqual(customerId, result.CustomerID);
        //    Assert.AreEqual(existingOrder.CreatedDate, result.CreatedDate);
        //    Assert.AreEqual(existingOrder.CreatedBy, result.CreatedBy);
        //    _mockDbContext.Verify(c => c.SaveChangesAsync(), Times.Once);
        //}

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdatePurchaseOrderAsync_WithNullOrder_ThrowsArgumentNullException()
        {
            // Act
            await _service.UpdatePurchaseOrderAsync(null);

            // Assert is handled by ExpectedException attribute
        }

        //[TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task UpdatePurchaseOrderAsync_WithNonExistentOrder_ThrowsKeyNotFoundException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationDbContext>>();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using var context = new ApplicationDbContext(options, mockLogger.Object);

            // Add a valid purchase order to the database
            var validOrder = new PurchaseOrder { OrderID = 1, CustomerID = 101 };
            context.PurchaseOrders.Add(validOrder);
            await context.SaveChangesAsync();

            var service = new PurchaseOrderService(context, _mockLogger.Object);

            // Create a non-existent order to update
            var nonExistentOrder = new PurchaseOrder { OrderID = 999, CustomerID = 102 };

            // Act
            await service.UpdatePurchaseOrderAsync(nonExistentOrder);

            // Assert is handled by the ExpectedException attribute
        }
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _innerEnumerator;

        public TestAsyncEnumerator(IEnumerator<T> innerEnumerator)
        {
            _innerEnumerator = innerEnumerator;
        }

        public T Current => _innerEnumerator.Current;

        public ValueTask DisposeAsync()
        {
            _innerEnumerator.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_innerEnumerator.MoveNext());
        }
    }
}

