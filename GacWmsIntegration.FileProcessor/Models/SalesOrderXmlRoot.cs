
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace GacWmsIntegration.FileProcessor.Models
{
    [XmlRoot("SalesOrders")]
    [ExcludeFromCodeCoverage]
    public class SalesOrderXmlRoot
    {
        [XmlElement("SalesOrder")]
        public List<SalesOrderXml> SalesOrders { get; set; } = new List<SalesOrderXml>();
    }

    [ExcludeFromCodeCoverage]
    public class SalesOrderXml
    {
        [XmlElement("OrderID")]
        public int OrderID { get; set; }

        [XmlElement("ProcessingDate")]
        public DateTime ProcessingDate { get; set; }

        [XmlElement("CustomerID")]
        public int CustomerID { get; set; }

        [XmlElement("ShipmentAddress")]
        public string ShipmentAddress { get; set; } = string.Empty;

        [XmlArray("OrderDetails")]
        [XmlArrayItem("OrderDetail")]
        public List<OrderDetailXml> OrderDetails { get; set; } = new List<OrderDetailXml>();
    }
}
