using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO
{
    public class CategoryDTO
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public IFormFile? Image { get; set; }
    }
}
