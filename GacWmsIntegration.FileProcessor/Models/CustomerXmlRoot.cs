
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace GacWmsIntegration.FileProcessor.Models
{
    [XmlRoot("Customers")]
    [ExcludeFromCodeCoverage]
    public class CustomerXmlRoot
    {
        [XmlElement("Customer")]
        public List<CustomerXml> Customers { get; set; } = new List<CustomerXml>();
    }

    [ExcludeFromCodeCoverage]
    public class CustomerXml
    {
        [XmlElement("CustomerID")]
        public int CustomerID { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; } = string.Empty;

        [XmlElement("Address")]
        public string Address { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class OrderDetailXml
    {
        [XmlElement("ProductCode")]
        public string ProductCode { get; set; } = string.Empty;

        [XmlElement("Quantity")]
        public int Quantity { get; set; }
    }
}
