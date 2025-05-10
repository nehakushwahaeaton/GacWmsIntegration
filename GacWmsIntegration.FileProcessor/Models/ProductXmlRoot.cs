
using System.Xml.Serialization;

namespace GacWmsIntegration.FileProcessor.Models
{
    [XmlRoot("Products")]
    public class ProductXmlRoot
    {
        [XmlElement("Product")]
        public List<ProductXml> Products { get; set; } = new List<ProductXml>();
    }

    public class ProductXml
    {
        [XmlElement("ProductCode")]
        public string ProductCode { get; set; } = string.Empty;

        [XmlElement("Title")]
        public string Title { get; set; } = string.Empty;

        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;

        [XmlElement("Dimensions")]
        public string Dimensions { get; set; } = string.Empty;
    }
}
