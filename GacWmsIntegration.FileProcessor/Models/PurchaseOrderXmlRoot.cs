
using System.Xml.Serialization;

namespace GacWmsIntegration.FileProcessor.Models
{
    [XmlRoot("PurchaseOrders")]
    public class PurchaseOrderXmlRoot
    {
        [XmlElement("PurchaseOrder")]
        public List<PurchaseOrderXml> PurchaseOrders { get; set; } = new List<PurchaseOrderXml>();
    }

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
