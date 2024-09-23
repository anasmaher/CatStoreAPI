using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.Models.AuthModels
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string password { get; set; }

        [MaxLength(255)]
        public string email { get; set; }

        [MaxLength(100)]
        public string role { get; set; }

        [MaxLength(100)]
        public string firstName { get; set; }
        
        [MaxLength(100)]
        public string lastName { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ShoppingCart shoppingCart { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual WishList wishlist { get; set; }
    }
}
