using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Models
{
    public class Review
    { 
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual Product Product { get; set; }

        [ForeignKey(nameof(ProductId))]
        public int ProductId { get; set; }

        [ForeignKey(nameof(UserId))]
        public string UserId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual AppUser User { get; set; }
    }
}
