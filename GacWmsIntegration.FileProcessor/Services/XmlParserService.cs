using Microsoft.Extensions.Logging;
using System.Xml.Serialization;
using System.Xml;
using GacWmsIntegration.Core.Models;
using GacWmsIntegration.FileProcessor.Models;

namespace GacWmsIntegration.FileProcessor.Services
{
    public class XmlParserService
    {
        private readonly ILogger<XmlParserService> _logger;

        public XmlParserService(ILogger<XmlParserService> logger)
        {
            _logger = logger;
        }

        public T? DeserializeXml<T>(string filePath)
        {
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open);
                var serializer = new XmlSerializer(typeof(T));
                return (T?)serializer.Deserialize(fileStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing XML file {FilePath}", filePath);
                return default;
            }
        }

        public List<Customer> ParseCustomers(string filePath)
        {
            var root = DeserializeXml<CustomerXmlRoot>(filePath);
            if (root == null || !root.Customers.Any())
            {
                _logger.LogWarning("No customers found in file {FilePath}", filePath);
                return new List<Customer>();
            }

            return root.Customers.Select(c => new Customer
            {
                CustomerID = c.CustomerID,
                Name = c.Name,
                Address = c.Address
            }).ToList();
        }

        public List<Product> ParseProducts(string filePath)
        {
            var root = DeserializeXml<ProductXmlRoot>(filePath);
            if (root == null || !root.Products.Any())
            {
                _logger.LogWarning("No products found in file {FilePath}", filePath);
                return new List<Product>();
            }

            return root.Products.Select(p => new Product
            {
                ProductCode = p.ProductCode,
                Title = p.Title,
                Description = p.Description,
                Dimensions = p.Dimensions
            }).ToList();
        }

        public List<PurchaseOrder> ParsePurchaseOrders(string filePath)
        {
            var root = DeserializeXml<PurchaseOrderXmlRoot>(filePath);
            if (root == null || !root.PurchaseOrders.Any())
            {
                _logger.LogWarning("No purchase orders found in file {FilePath}", filePath);
                return new List<PurchaseOrder>();
            }

            return root.PurchaseOrders.Select(po => new PurchaseOrder
            {
                OrderID = po.OrderID,
                ProcessingDate = po.ProcessingDate,
                CustomerID = po.CustomerID,
                Items = po.OrderDetails.Select(od => new PurchaseOrderDetails
                {
                    ProductCode = od.ProductCode,
                    Quantity = od.Quantity
                }).ToList()
            }).ToList();
        }

        public List<SalesOrder> ParseSalesOrders(string filePath)
        {
            var root = DeserializeXml<SalesOrderXmlRoot>(filePath);
            if (root == null || !root.SalesOrders.Any())
            {
                _logger.LogWarning("No sales orders found in file {FilePath}", filePath);
                return new List<SalesOrder>();
            }

            return root.SalesOrders.Select(so => new SalesOrder
            {
                OrderID = so.OrderID,
                ProcessingDate = so.ProcessingDate,
                CustomerID = so.CustomerID,
                ShipmentAddress = so.ShipmentAddress,
                Items = so.OrderDetails.Select(od => new SalesOrderDetails
                {
                    ProductCode = od.ProductCode,
                    Quantity = od.Quantity
                }).ToList()
            }).ToList();
        }
    }
}
