using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.AuthDTOs
{
    public class ResetPasswordDTO
    {
        [Required]
        [EmailAddress, MaxLength(255)]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [PasswordPropertyText, MaxLength(255)]
        public string NewPassword { get; set; }
    }
}
