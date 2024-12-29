using Core.Models;

namespace Core.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(AppUser user);
        Task StoreTokenAsync(string userId, string tokenId, DateTime expirationTime);
        Task RevokeTokenAsync(string tokenId);
        Task<bool> IsTokenRevokedAsync(string tokenId);
        Task CleanupExpiredTokensAsync();
    }
}
