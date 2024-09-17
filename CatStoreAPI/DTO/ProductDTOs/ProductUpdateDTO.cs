using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.ProductDTOs
{
    public class ProductUpdateDTO
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(1000)]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; }

        [MaxLength(100)]
        public string? LifeStage { get; set; }

        [Required, MaxLength(500)]
        public string ProductCode { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}
