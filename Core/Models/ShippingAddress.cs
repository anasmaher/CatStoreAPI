using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Models
{
    public class ShippingAddress
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual Order Order { get; set; }

        [MaxLength(100)]
        public string RecipientName { get; set; }

        [MaxLength(200)]
        public string Street { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(50)]
        public string State { get; set; }

        [MaxLength(20)]
        public string PostalCode { get; set; }

        [MaxLength(50)]
        public string Country { get; set; }
    }
}
