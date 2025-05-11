using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GacWmsIntegration.DTOs
{
    [ExcludeFromCodeCoverage]
    public class SalesOrderDto
    {
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? ExpectedShipDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [StringLength(50)]
        public string ReferenceNumber { get; set; }

        [StringLength(50)]
        public string CustomerID { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        [StringLength(50)]
        public string ShippingMethod { get; set; }

        [StringLength(200)]
        public string ShippingAddress { get; set; }

        [StringLength(100)]
        public string ShippingCity { get; set; }

        [StringLength(50)]
        public string ShippingState { get; set; }

        [StringLength(20)]
        public string ShippingPostalCode { get; set; }

        [StringLength(50)]
        public string ShippingCountry { get; set; }

        [StringLength(100)]
        public string BillingAddress { get; set; }

        [StringLength(100)]
        public string BillingCity { get; set; }

        [StringLength(50)]
        public string BillingState { get; set; }

        [StringLength(20)]
        public string BillingPostalCode { get; set; }

        [StringLength(50)]
        public string BillingCountry { get; set; }

        public decimal SubTotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ShippingAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public List<SalesOrderItemDto> Items { get; set; } = new List<SalesOrderItemDto>();
    }

    public class SalesOrderItemDto
    {
        public string Id { get; set; }

        public string SalesOrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TaxRate { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal DiscountRate { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TotalPrice { get; set; }

        public int LineNumber { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }

    public class SalesOrderCreateDto
    {
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpectedShipDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Draft";

        [StringLength(500)]
        public string Notes { get; set; }

        [StringLength(50)]
        public string ReferenceNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string CustomerID { get; set; }

        [StringLength(50)]
        public string ShippingMethod { get; set; }

        [StringLength(200)]
        public string ShippingAddress { get; set; }

        [StringLength(100)]
        public string ShippingCity { get; set; }

        [StringLength(50)]
        public string ShippingState { get; set; }

        [StringLength(20)]
        public string ShippingPostalCode { get; set; }

        [StringLength(50)]
        public string ShippingCountry { get; set; }

        [StringLength(100)]
        public string BillingAddress { get; set; }

        [StringLength(100)]
        public string BillingCity { get; set; }

        [StringLength(50)]
        public string BillingState { get; set; }

        [StringLength(20)]
        public string BillingPostalCode { get; set; }

        [StringLength(50)]
        public string BillingCountry { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ShippingAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        public List<SalesOrderItemCreateDto> Items { get; set; } = new List<SalesOrderItemCreateDto>();
    }

    public class SalesOrderItemCreateDto
    {
        [Required]
        [StringLength(50)]
        public string ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TaxRate { get; set; }

        public decimal DiscountRate { get; set; }

        public int LineNumber { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }

    public class SalesOrderUpdateDto
    {
        [StringLength(50)]
        public string Status { get; set; }

        public DateTime? ExpectedShipDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [StringLength(50)]
        public string ReferenceNumber { get; set; }

        [StringLength(50)]
        public string ShippingMethod { get; set; }

        [StringLength(200)]
        public string ShippingAddress { get; set; }

        [StringLength(100)]
        public string ShippingCity { get; set; }

        [StringLength(50)]
        public string ShippingState { get; set; }

        [StringLength(20)]
        public string ShippingPostalCode { get; set; }

        [StringLength(50)]
        public string ShippingCountry { get; set; }

        [StringLength(100)]
        public string BillingAddress { get; set; }

        [StringLength(100)]
        public string BillingCity { get; set; }

        [StringLength(50)]
        public string BillingState { get; set; }

        [StringLength(20)]
        public string BillingPostalCode { get; set; }

        [StringLength(50)]
        public string BillingCountry { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ShippingAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; }

        public DateTime? PaymentDate { get; set; }

        public List<SalesOrderItemUpdateDto> Items { get; set; } = new List<SalesOrderItemUpdateDto>();
    }

    public class SalesOrderItemUpdateDto
    {
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TaxRate { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal DiscountRate { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TotalPrice { get; set; }

        public int LineNumber { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }

    public class SalesOrderStatusUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
    }

    public class SalesOrderInventoryCheckResultDto
    {
        public string SalesOrderId { get; set; }

        public string OrderNumber { get; set; }

        public bool AllItemsInStock { get; set; }

        public List<SalesOrderItemInventoryStatusDto> ItemInventoryStatus { get; set; } = new List<SalesOrderItemInventoryStatusDto>();
    }

    public class SalesOrderItemInventoryStatusDto
    {
        public string ProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public decimal OrderedQuantity { get; set; }

        public decimal AvailableQuantity { get; set; }

        public bool IsInStock { get; set; }

        public decimal ShortageQuantity { get; set; }
    }
}
