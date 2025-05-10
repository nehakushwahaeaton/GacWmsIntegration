using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GacWmsIntegration.Core.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int OrderID { get; set; } 
        public DateTime ProcessingDate { get; set; } = DateTime.UtcNow;
        [ForeignKey("CustomerID")]
        public int CustomerID { get; set; } 
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty; 

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<PurchaseOrderDetails> Items { get; set; } = new List<PurchaseOrderDetails>(); // Changed to match SQL table
    }

    public class PurchaseOrderDetails
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; } 
        public string ProductCode { get; set; } = string.Empty; 
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrderID")]
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        [ForeignKey("ProductCode")]
        public virtual Product Product { get; set; } = null!;
    }
}
