using Core.Interfaces;
using Core.Models;
using Infrastructure.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration config;
        private readonly UserManager<AppUser> userManager;
        private readonly AppDbContext _dbContext;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager, AppDbContext _dbContext)
        {
            this.config = config;
            this.userManager = userManager;
            this._dbContext = _dbContext;
        }

        public async Task<string> GenerateTokenAsync(AppUser user)
        {
            var tokenId = Guid.NewGuid().ToString();

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, tokenId),
                new Claim("TokenVersion", user.TokenVersion.ToString())
            };

            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]));
            SigningCredentials creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: config["JWT:Issuer"],
                audience: config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(config["JWT:DurationInMinutes"])),
                signingCredentials: creds
            );

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

            await StoreTokenAsync(user.Id, tokenId, DateTime.UtcNow.AddMinutes(int.Parse(config["JWT:DurationInMinutes"])));
            return token;
        }

        public async Task StoreTokenAsync(string userId, string tokenId, DateTime expirationTime)
        {
            var token = new Token
            {
                TokenId = tokenId,
                UserId = userId,
                ExpirationTime = expirationTime,
                Revoked = false
            };
            _dbContext.Tokens.Add(token);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RevokeTokenAsync(string tokenId)
        {
            var token = await _dbContext.Tokens.FirstOrDefaultAsync(t => t.TokenId == tokenId);
            if (token is not null)
            {
                token.Revoked = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> IsTokenRevokedAsync(string tokenId)
        {
            var token = await _dbContext.Tokens.FirstOrDefaultAsync(t => t.TokenId == tokenId);
            return token?.Revoked ?? false;
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = _dbContext.Tokens.Where(t => t.ExpirationTime <= DateTime.UtcNow);
            _dbContext.Tokens.RemoveRange(expiredTokens);
            await _dbContext.SaveChangesAsync();
        }
    }
}
