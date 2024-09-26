using Core.Models;

namespace Core.Interfaces
{
    public interface IUserService
    {
        public Task<AppUser> FindUserAsync(string email, string password);
    }
}
