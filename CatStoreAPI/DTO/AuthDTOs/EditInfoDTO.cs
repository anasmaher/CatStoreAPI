using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.AuthDTOs
{
    public class EditInfoDTO
    {
        [Required, MaxLength(255)]
        public string FirstName { get; set; }

        [Required, MaxLength(255)]
        public string LastName { get; set; }
    }
}
