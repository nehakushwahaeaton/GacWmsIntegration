using GacWmsIntegration.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GacWmsIntegration.DTOs
{
    // Represents a product entity with all its properties
    [ExcludeFromCodeCoverage]
    public class ProductDto
    {
        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; }  

        [Required]
        [StringLength(100)]
        public string Title { get; set; }  

        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Dimensions { get; set; }  

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }

    // Used for creating new products
    [ExcludeFromCodeCoverage]
    public class ProductCreateDto
    {
        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; }  

        [Required]
        [StringLength(100)]
        public string Title { get; set; }  

        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Dimensions { get; set; }  
    }

    // Used for updating existing products
    [ExcludeFromCodeCoverage]
    public class ProductUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }  

        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Dimensions { get; set; }  

        public DateTime? ModifiedDate { get; set; }
    }

    // Used to return inventory information for a product
    [ExcludeFromCodeCoverage]
    public class ProductInventoryDto
    {
        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; }  

        [Required]
        [StringLength(100)]
        public string Title { get; set; }  

        public decimal AvailableQuantity { get; set; }

        public decimal AllocatedQuantity { get; set; }

        public decimal TotalQuantity { get; set; }

        [StringLength(50)]
        public string UnitOfMeasure { get; set; }
    }
}
