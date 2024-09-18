using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.ProductDTOs
{
    public class ProductUpdateDTO
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        public decimal? Price { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(100)]
        public string? CategoryName { get; set; }

        [MaxLength(100)]
        public string? LifeStage { get; set; }

        [MaxLength(500)]
        public string? ProductCode { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}
