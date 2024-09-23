using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.AuthDTOs
{
    public class AuthRegisterDTO
    {
        [Required]
        [MaxLength(100)]
        public string firstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string lastName { get; set; }

        [Required]
        [MaxLength(255), EmailAddress]
        public string email { get; set; }

        [Required]
        [MaxLength(100), PasswordPropertyText]
        public string password { get; set; }
    }
}
