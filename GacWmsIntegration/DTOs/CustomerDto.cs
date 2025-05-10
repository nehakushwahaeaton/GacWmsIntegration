using System;
using System.ComponentModel.DataAnnotations;

namespace GacWmsIntegration.DTOs
{
    // Represents a customer entity with all its properties
    public class CustomerDto
    {
        public int CustomerID { get; set; } 

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }


    public class CustomerCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Address { get; set; }
    }


    public class CustomerUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
