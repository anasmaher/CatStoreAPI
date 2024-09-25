using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Models
{
    public class WishList
    {
        public int Id { get; set; }

        public virtual List<Product> Products { get; set; } = new List<Product>();

        [ForeignKey(nameof(userId))]
        public string userId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual AppUser user { get; set; }
    }
}
