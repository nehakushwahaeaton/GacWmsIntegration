using System.ComponentModel.DataAnnotations;

namespace GacWmsIntegration.DTOs
{
    // Represents a purchase order with its header information and line items
    public class PurchaseOrderDto
    {
        public int OrderID { get; set; }  

        public DateTime ProcessingDate { get; set; }  

        [Required]
        public int CustomerID { get; set; }  

        [StringLength(255)]
        public string ShipmentAddress { get; set; }  

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public List<PurchaseOrderItemDto> Items { get; set; } = new List<PurchaseOrderItemDto>();
    }

    // Represents a line item in a purchase order
    public class PurchaseOrderItemDto
    {
        public int OrderDetailID { get; set; }  

        [Required]
        public int OrderID { get; set; }  

        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; }  

        public int Quantity { get; set; }  
    }

    // Used for creating new purchase orders
    public class PurchaseOrderCreateDto
    {
        [Required]
        public DateTime ProcessingDate { get; set; } = DateTime.UtcNow;  

        [Required]
        public int CustomerID { get; set; }  

        [StringLength(255)]
        public string ShipmentAddress { get; set; }  

        [Required]
        public List<PurchaseOrderItemCreateDto> Items { get; set; } = new List<PurchaseOrderItemCreateDto>();
    }

    // Used for creating purchase order line items
    public class PurchaseOrderItemCreateDto
    {
        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; }  

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }  
    }

    // Used for updating existing purchase orders
    public class PurchaseOrderUpdateDto
    {
        [StringLength(255)]
        public string ShipmentAddress { get; set; }  

        [Required]
        public List<PurchaseOrderItemUpdateDto> Items { get; set; } = new List<PurchaseOrderItemUpdateDto>();
    }

    // Used for updating purchase order line items
    public class PurchaseOrderItemUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; }  

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }  
    }

    // Used for updating just the status of a purchase order
    public class PurchaseOrderStatusUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
    }
}
