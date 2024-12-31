using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.OrderDTOs
{
    public class ShippingAddressDTO
    {
        [Required]
        [MaxLength(100)]
        public string RecipientName { get; set; }

        [Required]
        [MaxLength(200)]
        public string Street { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(50)]
        public string State { get; set; }

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string Country { get; set; }
    }
}
