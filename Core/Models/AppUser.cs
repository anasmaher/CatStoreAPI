using Microsoft.AspNetCore.Identity;

namespace Core.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public virtual ShoppingCart ShoppingCart { get; set; }

        public virtual WishList WishList { get; set; }

        public int TokenVersion { get; set; } = 1;

        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
