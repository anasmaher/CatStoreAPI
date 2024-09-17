using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.CategoryDTOs
{
    public class CategoryDTO
    {
        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public string? ImageUrl { get; set; }
    }
}
