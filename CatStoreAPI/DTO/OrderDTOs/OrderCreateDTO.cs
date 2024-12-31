using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.OrderDTOs
{
    public class OrderCreateDTO
    {
        [Required]
        public ShippingAddressDTO ShippingAddress { get; set; }

        [Required]
        public string PaymentMethodId { get; set; }
    }
}
