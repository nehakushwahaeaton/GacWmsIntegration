using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace GacWmsIntegration.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class SalesOrder
    {
        [Key]
        public int OrderID { get; set; }  
        public DateTime ProcessingDate { get; set; } = DateTime.UtcNow;  
        public int CustomerID { get; set; }  
        public string ShipmentAddress { get; set; } = string.Empty;  
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("CustomerID")]
        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<SalesOrderDetails> Items { get; set; } = new List<SalesOrderDetails>();  
    }

    [ExcludeFromCodeCoverage]
    public class SalesOrderDetails
    {
        [Key]
        public int OrderDetailID { get; set; }  
        public int OrderID { get; set; }  
        public string ProductCode { get; set; } = string.Empty;  
        public int Quantity { get; set; }  
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrderID")]
        public virtual SalesOrder SalesOrder { get; set; } = null!;
        [ForeignKey("ProductCode")]
        public virtual Product Product { get; set; } = null!;
    }
}
