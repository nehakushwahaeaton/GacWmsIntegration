using GacWmsIntegration.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GacWmsIntegration.FileProcessor.Interfaces
{
    public interface IXmlParserService
    {
        T? DeserializeXml<T>(string filePath);
        List<Customer> ParseCustomers(string filePath);
        List<Product> ParseProducts(string filePath);
        List<PurchaseOrder> ParsePurchaseOrders(string filePath);
        List<SalesOrder> ParseSalesOrders(string filePath);
    }
}
