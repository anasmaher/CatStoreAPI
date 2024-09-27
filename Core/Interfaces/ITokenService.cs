using Core.Models;

namespace Core.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(AppUser user);
    }
}
