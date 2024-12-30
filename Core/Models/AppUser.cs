using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class AppUser : IdentityUser
    {
        [MaxLength(200)]
        public string FirstName { get; set; }

        [MaxLength(200)]
        public string LastName { get; set; }

        public virtual ShoppingCart ShoppingCart { get; set; }

        public virtual WishList WishList { get; set; }

        public virtual List<Review> Reviews { get; set; }

        public int TokenVersion { get; set; } = 1;

        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
