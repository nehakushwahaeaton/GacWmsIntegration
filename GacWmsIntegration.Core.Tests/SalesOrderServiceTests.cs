using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using GacWmsIntegration.Core.Services;
using GacWmsIntegration.Core.Tests.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using static GacWmsIntegration.Core.Tests.Services.ProductServiceTests;

namespace GacWmsIntegration.Core.Tests
{
    [TestClass]
    public class SalesOrderServiceTests
    {
        private Mock<IApplicationDbContext> _mockDbContext;
        private Mock<ILogger<SalesOrderService>> _mockLogger;
        private SalesOrderService _salesOrderService;
        private Mock<DbSet<SalesOrder>> _mockSalesOrderDbSet;
        private Mock<DbSet<Customer>> _mockCustomerDbSet;
        private Mock<DbSet<SalesOrderDetails>> _mockSalesOrderDetailDbSet;

        [TestInitialize]
        public void Setup()
        {
            _mockDbContext = new Mock<IApplicationDbContext>();
            _mockLogger = new Mock<ILogger<SalesOrderService>>();

            // Create mock DbSets
            _mockSalesOrderDbSet = new Mock<DbSet<SalesOrder>>();
            _mockCustomerDbSet = new Mock<DbSet<Customer>>();
            _mockSalesOrderDetailDbSet = new Mock<DbSet<SalesOrderDetails>>();

            // Setup DbContext to return mock DbSets
            _mockDbContext.Setup(db => db.SalesOrders).Returns(_mockSalesOrderDbSet.Object);
            _mockDbContext.Setup(db => db.Customers).Returns(_mockCustomerDbSet.Object);
            _mockDbContext.Setup(db => db.SalesOrderDetails).Returns(_mockSalesOrderDetailDbSet.Object);

            _salesOrderService = new SalesOrderService(_mockDbContext.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetAllSalesOrdersAsync_ReturnsSalesOrders()
        {
            // Arrange
            var mockSalesOrders = new List<SalesOrder>
    {
        new SalesOrder
        {
            OrderID = 1,
            Customer = new Customer { CustomerID = 1, Name = "Customer 1" },
            Items = new List<SalesOrderDetails>
            {
                new SalesOrderDetails
                {
                    Product = new Product { ProductCode = "P001", Title = "Product 1" },
                    Quantity = 2
                }
            }
        },
        new SalesOrder
        {
            OrderID = 2,
            Customer = new Customer { CustomerID = 2, Name = "Customer 2" },
            Items = new List<SalesOrderDetails>
            {
                new SalesOrderDetails
                {
                    Product = new Product { ProductCode = "P002", Title = "Product 2" },
                    Quantity = 1
                }
            }
        }
    }.AsQueryable();

            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<SalesOrder>(mockSalesOrders.Provider));
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(mockSalesOrders.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(mockSalesOrders.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(mockSalesOrders.GetEnumerator());
            mockDbSet.As<IAsyncEnumerable<SalesOrder>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<SalesOrder>(mockSalesOrders.GetEnumerator()));

            _mockDbContext.Setup(c => c.SalesOrders).Returns(mockDbSet.Object);

            // Act
            var result = await _salesOrderService.GetAllSalesOrdersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Customer 1", result.First().Customer.Name);
            Assert.AreEqual("Product 1", result.First().Items.First().Product.Title);
        }

        //[TestMethod]
        public async Task GetSalesOrderByIdAsync_ReturnsSalesOrder_WhenFound()
        {
            // Arrange
            var salesOrder = new SalesOrder
            {
                OrderID = 1,
                CustomerID = 1,
                ShipmentAddress = "Address 1",
                Customer = new Customer { CustomerID = 1, Name = "Test Customer" },
                Items = new List<SalesOrderDetails>
        {
            new SalesOrderDetails { OrderDetailID = 1, OrderID = 1, ProductCode = "P001", Quantity = 5 }
        }
            };

            var mockSalesOrders = new List<SalesOrder> { salesOrder }.AsQueryable();

            // Ensure all .As<TInterface>() calls are made before accessing the Object property
            _mockSalesOrderDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<SalesOrder>(mockSalesOrders.Provider));
            _mockSalesOrderDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(mockSalesOrders.Expression);
            _mockSalesOrderDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(mockSalesOrders.ElementType);
            _mockSalesOrderDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(mockSalesOrders.GetEnumerator());
            _mockSalesOrderDbSet.As<IAsyncEnumerable<SalesOrder>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<SalesOrder>(mockSalesOrders.GetEnumerator()));

            // Mock the FindAsync method
            _mockSalesOrderDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] ids) => mockSalesOrders.FirstOrDefault(s => s.OrderID == (int)ids[0]));

            // Act
            var result = await _salesOrderService.GetSalesOrderByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.OrderID);
            Assert.IsNotNull(result.Customer);
            Assert.AreEqual(1, result.Customer.CustomerID);
            Assert.IsNotNull(result.Items);
            Assert.AreEqual(1, result.Items.Count());
        }

        //[TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetSalesOrderByIdAsync_ThrowsException_WhenNotFound()
        {
            // Arrange
            var salesOrders = new List<SalesOrder>().AsQueryable();

            // Set up the basic IQueryable implementation
            _mockSalesOrderDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(salesOrders.Provider);
            _mockSalesOrderDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(salesOrders.Expression);
            _mockSalesOrderDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(salesOrders.ElementType);
            _mockSalesOrderDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(salesOrders.GetEnumerator());

            // Mock the FindAsync method to return null
            _mockSalesOrderDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((SalesOrder?)null); // Explicitly mark the type as nullable

            // Act
            await _salesOrderService.GetSalesOrderByIdAsync(1);
        }

        //[TestMethod]
        public async Task CreateSalesOrderAsync_AddsSalesOrder()
        {
            // Arrange
            var salesOrder = new SalesOrder
            {
                OrderID = 1,
                CustomerID = 1,
                ShipmentAddress = "Address 1",
                CreatedDate = DateTime.Now
            };

            // Setup customer exists check
            _mockCustomerDbSet.Setup(m => m.AnyAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Setup order ID uniqueness check
            _mockSalesOrderDbSet.Setup(m => m.AnyAsync(
                It.IsAny<Expression<Func<SalesOrder, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Setup AddAsync
            _mockSalesOrderDbSet.Setup(m => m.AddAsync(It.IsAny<SalesOrder>(), It.IsAny<CancellationToken>()))
                .Callback<SalesOrder, CancellationToken>((order, _) => { })
                .ReturnsAsync((SalesOrder order, CancellationToken _) => {
                    var mockEntry = new Mock<EntityEntry<SalesOrder>>();
                    mockEntry.Setup(e => e.Entity).Returns(order);
                    return mockEntry.Object;
                });

            // Setup SaveChangesAsync
            _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _salesOrderService.CreateSalesOrderAsync(salesOrder);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.OrderID);

            // Verify AddAsync was called
            _mockSalesOrderDbSet.Verify(m => m.AddAsync(
                It.Is<SalesOrder>(s => s.OrderID == 1),
                It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify SaveChangesAsync was called
            _mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
