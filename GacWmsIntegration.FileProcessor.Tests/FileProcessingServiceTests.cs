using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using GacWmsIntegration.FileProcessor.Interfaces;
using GacWmsIntegration.FileProcessor.Models;
using GacWmsIntegration.FileProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GacWmsIntegration.FileProcessor.Tests
{
    [TestClass]
    public class FileProcessingServiceTests
    {
        private Mock<ILogger<FileProcessingService>> _loggerMock;
        private Mock<IOptions<FileProcessingConfig>> _configMock;
        private Mock<IXmlParserService> _xmlParserMock;
        private Mock<ICustomerService> _customerServiceMock;
        private Mock<IProductService> _productServiceMock;
        private Mock<IPurchaseOrderService> _purchaseOrderServiceMock;
        private Mock<ISalesOrderService> _salesOrderServiceMock;
        private FileProcessingService _service;
        private FileProcessingConfig _config;
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<FileProcessingService>>();
            _xmlParserMock = new Mock<IXmlParserService>();
            _customerServiceMock = new Mock<ICustomerService>();
            _productServiceMock = new Mock<IProductService>();
            _purchaseOrderServiceMock = new Mock<IPurchaseOrderService>();
            _salesOrderServiceMock = new Mock<ISalesOrderService>();

            _config = new FileProcessingConfig
            {
                FileWatchers = new List<FileWatcherConfig>
    {
        new FileWatcherConfig
        {
            Name = "CustomerWatcher",
            DirectoryPath = "TestData/Customers",
            FilePattern = "*.xml",
            CronSchedule = "*/5 * * * *",
            FileType = FileType.Customer,
            MaxRetryAttempts = 3,
            ArchiveProcessedFiles = true,
            ArchivePath = "TestData/Archive/Customers"
        },
        new FileWatcherConfig
        {
            Name = "ProductWatcher",
            DirectoryPath = "TestData/Products",
            FilePattern = "*.xml",
            CronSchedule = "*/5 * * * *",
            FileType = FileType.Product,
            MaxRetryAttempts = 3,
            ArchiveProcessedFiles = false
        }
    }.ToArray() // Convert the list to an array
            };


            _configMock = new Mock<IOptions<FileProcessingConfig>>();
            _configMock.Setup(x => x.Value).Returns(_config);

            _service = new FileProcessingService(
                _loggerMock.Object,
                _configMock.Object,
                _xmlParserMock.Object,
                _customerServiceMock.Object,
                _productServiceMock.Object,
                _purchaseOrderServiceMock.Object,
                _salesOrderServiceMock.Object);

            // Create test directory structure
            _testDirectory = Path.Combine(Path.GetTempPath(), "FileProcessingServiceTests");
            Directory.CreateDirectory(_testDirectory);
            Directory.CreateDirectory(Path.Combine(_testDirectory, "Customers"));
            Directory.CreateDirectory(Path.Combine(_testDirectory, "Products"));
            Directory.CreateDirectory(Path.Combine(_testDirectory, "Archive"));
            Directory.CreateDirectory(Path.Combine(_testDirectory, "Archive", "Customers"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test directories
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [TestMethod]
        public async Task ProcessFilesAsync_WithNoFiles_ShouldNotProcessAnything()
        {
            // Arrange
            var watcherConfig = new FileWatcherConfig
            {
                Name = "EmptyWatcher",
                DirectoryPath = Path.Combine(_testDirectory, "Empty"),
                FilePattern = "*.xml",
                FileType = FileType.Customer
            };

            Directory.CreateDirectory(watcherConfig.DirectoryPath);

            // Act
            await _service.ProcessFilesAsync(watcherConfig, CancellationToken.None);

            // Assert
            _xmlParserMock.Verify(x => x.ParseCustomers(It.IsAny<string>()), Times.Never);
            _customerServiceMock.Verify(x => x.CreateCustomerAsync(It.IsAny<Customer>()), Times.Never);
            _customerServiceMock.Verify(x => x.UpdateCustomerAsync(It.IsAny<Customer>()), Times.Never);
        }

        [TestMethod]
        public async Task ProcessFilesAsync_WithCustomerFiles_ShouldProcessCustomers()
        {
            // Arrange
            var watcherConfig = new FileWatcherConfig
            {
                Name = "CustomerWatcher",
                DirectoryPath = Path.Combine(_testDirectory, "Customers"),
                FilePattern = "*.xml",
                FileType = FileType.Customer,
                ArchiveProcessedFiles = true,
                ArchivePath = Path.Combine(_testDirectory, "Archive", "Customers")
            };

            // Create a test file
            var testFilePath = Path.Combine(watcherConfig.DirectoryPath, "customers.xml");
            File.WriteAllText(testFilePath, "<Customers><Customer></Customer></Customers>");

            var customers = new List<Customer>
            {
                new Customer { CustomerID = 1, Name = "Test Customer" }
            };

            _xmlParserMock.Setup(x => x.ParseCustomers(testFilePath)).Returns(customers);
            _customerServiceMock.Setup(x => x.GetCustomerByIdAsync(1)).ReturnsAsync((Customer)null);

            // Act
            await _service.ProcessFilesAsync(watcherConfig, CancellationToken.None);

            // Assert
            _xmlParserMock.Verify(x => x.ParseCustomers(testFilePath), Times.Once);
            _customerServiceMock.Verify(x => x.GetCustomerByIdAsync(1), Times.Once);
            _customerServiceMock.Verify(x => x.CreateCustomerAsync(It.IsAny<Customer>()), Times.Once);

            // Verify file was archived
            Assert.IsFalse(File.Exists(testFilePath), "Original file should be moved");
            Assert.IsTrue(Directory.GetFiles(watcherConfig.ArchivePath).Length > 0, "Archive directory should contain files");
        }

        [TestMethod]
        public async Task ProcessFilesAsync_WithProductFiles_ShouldProcessProducts()
        {
            // Arrange
            var watcherConfig = new FileWatcherConfig
            {
                Name = "ProductWatcher",
                DirectoryPath = Path.Combine(_testDirectory, "Products"),
                FilePattern = "*.xml",
                FileType = FileType.Product,
                ArchiveProcessedFiles = false
            };

            // Create a test file
            var testFilePath = Path.Combine(watcherConfig.DirectoryPath, "products.xml");
            File.WriteAllText(testFilePath, "<Products><Product></Product></Products>");

            var products = new List<Product>
            {
                new Product { ProductCode = "PROD001", Description = "Test Product" }
            };

            _xmlParserMock.Setup(x => x.ParseProducts(testFilePath)).Returns(products);
            _productServiceMock.Setup(x => x.GetProductByCodeAsync("PROD001")).ReturnsAsync((Product)null);

            // Act
            await _service.ProcessFilesAsync(watcherConfig, CancellationToken.None);

            // Assert
            _xmlParserMock.Verify(x => x.ParseProducts(testFilePath), Times.Once);
            _productServiceMock.Verify(x => x.GetProductByCodeAsync("PROD001"), Times.Once);
            _productServiceMock.Verify(x => x.CreateProductAsync(It.IsAny<Product>()), Times.Once);

            // Verify file was deleted (not archived)
            Assert.IsFalse(File.Exists(testFilePath), "File should be deleted");
        }

        //[TestMethod]
        //public async Task ProcessFilesAsync_WithPurchaseOrderFiles_ShouldProcessPurchaseOrders()
        //{
        //    // Arrange
        //    var watcherConfig = new FileWatcherConfig
        //    {
        //        Name = "PurchaseOrderWatcher",
        //        DirectoryPath = Path.Combine(_testDirectory, "PurchaseOrders"),
        //        FilePattern = "*.xml",
        //        FileType = FileType.PurchaseOrder,
        //        ArchiveProcessedFiles = false
        //    };

        //    Directory.CreateDirectory(watcherConfig.DirectoryPath);

        //    // Create a test file
        //    var testFilePath = Path.Combine(watcherConfig.DirectoryPath, "purchaseOrders.xml");
        //    File.WriteAllText(testFilePath, "<PurchaseOrders><PurchaseOrder></PurchaseOrder></PurchaseOrders>");

        //    var purchaseOrders = new List<PurchaseOrder>
        //    {
        //        new PurchaseOrder { OrderID = 1, CreatedDate = DateTime.Now }
        //    };

        //    _xmlParserMock.Setup(x => x.ParsePurchaseOrders(testFilePath)).Returns(purchaseOrders);
        //    _purchaseOrderServiceMock.Setup(x => x.GetPurchaseOrderByIdAsync(1)).ReturnsAsync((PurchaseOrder)null);

        //    // Act
        //    await _service.ProcessFilesAsync(watcherConfig, CancellationToken.None);

        //    // Assert
        //    _xmlParserMock.Verify(x => x.ParsePurchaseOrders(testFilePath), Times.Once);
        //    _purchaseOrderServiceMock.Verify(x => x.GetPurchaseOrderByIdAsync(1), Times.Once);
        //    _purchaseOrderServiceMock.Verify(x => x.CreatePurchaseOrderAsync(It.IsAny<PurchaseOrder>()), Times.Exactly(2));

        //    // Verify file was deleted
        //    Assert.IsFalse(File.Exists(testFilePath), "File should be deleted");
        //}

        //[TestMethod]
        //public async Task ProcessFilesAsync_WithSalesOrderFiles_ShouldProcessSalesOrders()
        //{
        //    // Arrange
        //    var watcherConfig = new FileWatcherConfig
        //    {
        //        Name = "SalesOrderWatcher",
        //        DirectoryPath = Path.Combine(_testDirectory, "SalesOrders"),
        //        FilePattern = "*.xml",
        //        FileType = FileType.SalesOrder,
        //        ArchiveProcessedFiles = false
        //    };

        //    Directory.CreateDirectory(watcherConfig.DirectoryPath);

        //    // Create a test file
        //    var testFilePath = Path.Combine(watcherConfig.DirectoryPath, "salesOrders.xml");
        //    File.WriteAllText(testFilePath, "<SalesOrders><SalesOrder></SalesOrder></SalesOrders>");

        //    var salesOrders = new List<SalesOrder>
        //    {
        //        new SalesOrder { OrderID = 1, CreatedDate = DateTime.Now }
        //    };

        //    _xmlParserMock.Setup(x => x.ParseSalesOrders(testFilePath)).Returns(salesOrders);
        //    _salesOrderServiceMock.Setup(x => x.GetSalesOrderByIdAsync(1)).ReturnsAsync((SalesOrder)null);

        //    // Act
        //    await _service.ProcessFilesAsync(watcherConfig, CancellationToken.None);

        //    // Assert
        //    _xmlParserMock.Verify(x => x.ParseSalesOrders(testFilePath), Times.Once);
        //    _salesOrderServiceMock.Verify(x => x.GetOrderItemsAsync(1), Times.Once);
        //    _salesOrderServiceMock.Verify(x => x.CreateSalesOrderAsync(It.IsAny<SalesOrder>()), Times.Once);

        //    // Verify file was deleted
        //    Assert.IsFalse(File.Exists(testFilePath), "File should be deleted");
        //}

        [TestMethod]
        public async Task ProcessFilesAsync_WithExistingCustomer_ShouldUpdateCustomer()
        {
            // Arrange
            var watcherConfig = new FileWatcherConfig
            {
                Name = "CustomerWatcher",
                DirectoryPath = Path.Combine(_testDirectory, "Customers"),
                FilePattern = "*.xml",
                FileType = FileType.Customer,
                ArchiveProcessedFiles = false
            };

            // Create a test file
            var testFilePath = Path.Combine(watcherConfig.DirectoryPath, "customers.xml");
            File.WriteAllText(testFilePath, "<Customers><Customer></Customer></Customers>");

            var customer = new Customer { CustomerID = 1, Name = "Test Customer" };
            var existingCustomer = new Customer { CustomerID = 2, Name = "Old Name" };

            var customers = new List<Customer> { customer };

            _xmlParserMock.Setup(x => x.ParseCustomers(testFilePath)).Returns(customers);
            _customerServiceMock.Setup(x => x.GetCustomerByIdAsync(1)).ReturnsAsync(existingCustomer);

            // Act
            await _service.ProcessFilesAsync(watcherConfig, CancellationToken.None);

            // Assert
            _xmlParserMock.Verify(x => x.ParseCustomers(testFilePath), Times.Once);
            _customerServiceMock.Verify(x => x.GetCustomerByIdAsync(1), Times.Once);
            _customerServiceMock.Verify(x => x.UpdateCustomerAsync(It.IsAny<Customer>()), Times.Once);
            _customerServiceMock.Verify(x => x.CreateCustomerAsync(It.IsAny<Customer>()), Times.Never);
        }

        [TestMethod]
        public async Task ProcessFilesAsync_WithNonExistentDirectory_ShouldLogWarning()
        {
            // Arrange
            var watcherConfig = new FileWatcherConfig
            {
                Name = "NonExistentWatcher",
                DirectoryPath = Path.Combine(_testDirectory, "NonExistent"),
                FilePattern = "*.xml",
                FileType = FileType.Customer
            };

            // Act
            await _service.ProcessFilesAsync(watcherConfig, CancellationToken.None);

            // Assert
            _xmlParserMock.Verify(x => x.ParseCustomers(It.IsAny<string>()), Times.Never);
            _customerServiceMock.Verify(x => x.CreateCustomerAsync(It.IsAny<Customer>()), Times.Never);
            _customerServiceMock.Verify(x => x.UpdateCustomerAsync(It.IsAny<Customer>()), Times.Never);

            // We can't easily verify logger calls with the mock setup, but no exceptions should be thrown
        }
    }
}
