using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.AuthDTOs
{
    public class ChangePasswordDTO
    {
        [Required]
        [PasswordPropertyText]
        public string CurrentPassword { get; set; }

        [Required]
        [PasswordPropertyText]
        public string NewPassword { get; set; }
    }
}
