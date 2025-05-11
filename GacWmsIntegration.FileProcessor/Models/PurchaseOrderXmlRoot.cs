
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace GacWmsIntegration.FileProcessor.Models
{
    [XmlRoot("PurchaseOrders")]
    [ExcludeFromCodeCoverage]
    public class PurchaseOrderXmlRoot
    {
        [XmlElement("PurchaseOrder")]
        public List<PurchaseOrderXml> PurchaseOrders { get; set; } = new List<PurchaseOrderXml>();
    }

    [ExcludeFromCodeCoverage]
    public class PurchaseOrderXml
    {
        [XmlElement("OrderID")]
        public int OrderID { get; set; }

        [XmlElement("ProcessingDate")]
        public DateTime ProcessingDate { get; set; }

        [XmlElement("CustomerID")]
        public int CustomerID { get; set; }

        [XmlArray("OrderDetails")]
        [XmlArrayItem("OrderDetail")]
        public List<OrderDetailXml> OrderDetails { get; set; } = new List<OrderDetailXml>();
    }
}
