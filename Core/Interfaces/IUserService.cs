using Core.Models;
using Microsoft.AspNetCore.Identity;

namespace Core.Interfaces
{
    public interface IUserService
    {
        public Task<AppUser> FindUserAsync(string email, string password);

        public Task<IdentityResult> CreateUserAsync(string firstName, string lastName, string email, string password);

        public Task DeleteUser();
    }
}
