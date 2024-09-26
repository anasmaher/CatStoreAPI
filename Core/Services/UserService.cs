using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Identity;

namespace Core.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<AppUser> FindUserAsync(string email, string password)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null || !await userManager.CheckPasswordAsync(user, password))
                return null;
            
            return user;
        }
    }
}
