using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }

        [ForeignKey(nameof(ShoppingCartId))]
        public int ShoppingCartId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ShoppingCart ShoppingCart { get; set; }

        [ForeignKey(nameof(ProductId))]
        public int ProductId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual Product Product { get; set; }

        public int Quantity { get; set; }
    }
}
