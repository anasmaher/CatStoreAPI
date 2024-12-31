using Core.Enums;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual AppUser User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public decimal TotalAmount { get; set; }

        public string PaymentIntentId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ShippingAddress ShippingAddress { get; set; }
    }
}
