using System.ComponentModel.DataAnnotations;

namespace CatStoreAPI.DTO.ProductDTOs
{
    public class ProductDTO
    {
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public decimal Price { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalPrice { get; set; }

        public string Brand { get; set; }

        public string CategoryName { get; set; }

        public string LifeStage { get; set; }

        public string ProductCode { get; set; }

        public string ImageUrl { get; set; }
    }
}
