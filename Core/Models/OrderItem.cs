using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual Order Order { get; set; }

        public int ProductId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual Product Product { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
