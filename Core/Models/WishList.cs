using Core.Models.AuthModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.Models
{
    public class WishList
    {
        public int Id { get; set; }

        public virtual List<Product> Products { get; set; } = new List<Product>();

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual User user { get; set; }
    }
}
