using CatStoreAPI.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Models
{
    public class Product
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public decimal Price { get; set; } = 0;

        public decimal TotalPrice 
        {
            get
            {
                return Price - Discount;
            }
        }

        public int StockAmount { get; set; }

        public decimal Discount { get; set; } = 0;

        [MaxLength(100)]
        public string Brand { get; set; }

        [MaxLength(100)]
        public string LifeStage { get; set; }

        [MaxLength(500)]
        public string ProductCode { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual List<ShoppingCartItem> Items { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual List<WishList> WhishLists { get; set; }

        public virtual List<Review> Reviews { get; set; }

        [NotMapped]
        public double AverageRating
        {
            get
            {
                if (Reviews is not null && Reviews.Any())
                {
                    return Reviews.Average(r => r.Rating);
                }
                return 0;
            }
        }
    }
}
