using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.CategoryDTOs
{
    public class CategoryCreatDTO
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}
