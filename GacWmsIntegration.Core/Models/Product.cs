
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GacWmsIntegration.Core.Models
{
    //ProductMaster  SQL table

    [Table("ProductMaster")]

    public class Product
    {
        [Key]
        public string ProductCode { get; set; } = string.Empty; // Changed to match SQL table
        public string Title { get; set; } = string.Empty; // Changed to match SQL table
        public string Description { get; set; } = string.Empty;
        public string Dimensions { get; set; } = string.Empty; // Added to match SQL table
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty; // Added to match SQL table
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
        public string ModifiedBy { get; set; } = string.Empty; // Added to match SQL table

    }
}
