using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.AuthDTOs
{
    public class AuthLoginDTO
    {
        [Required]
        [EmailAddress, MaxLength(255)]
        public string Email { get; set; }
        
        [Required]
        [PasswordPropertyText, MaxLength(100)]
        public string Password { get; set; }
    }
}
