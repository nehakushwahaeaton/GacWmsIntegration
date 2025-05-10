
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GacWmsIntegration.Core.Models
{
    //CustomerMaster  SQL table
    [Table("CustomerMaster")]
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty; 
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
        public string ModifiedBy { get; set; } = string.Empty; 

        // Navigation properties
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    }
}
