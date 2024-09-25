using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        public decimal price
        {
            get
            {
                decimal totalPrice = 0;
                if (Items is not null)
                    foreach (var item in Items)
                        totalPrice += item.price;

                return totalPrice;
            }
        }

        public virtual List<ShoppingCartItem> Items { get; set; }

        [ForeignKey(nameof(userId))]
        public string userId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual AppUser User { get; set; }

    }
}
