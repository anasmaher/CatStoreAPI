using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.AuthDTOs
{
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress, MaxLength(255)]
        public string Email { get; set; }
    }
}
