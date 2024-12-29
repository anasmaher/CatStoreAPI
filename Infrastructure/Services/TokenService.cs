using Core.Interfaces;
using Core.Models;
using Infrastructure.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        public async Task<AuthenticationResult> GenerateTokenAsync(AppUser user, bool generateRefreshToken = true)
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

            var result = new AuthenticationResult
            {
                Token = token
            };

            if (generateRefreshToken)
            {
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(int.Parse(config["JWT:DurationInMinutes"]));

                await userManager.UpdateAsync(user);

                result.RefreshToken = refreshToken;
            }

            await StoreTokenAsync(user.Id, tokenId, DateTime.UtcNow.AddMinutes(int.Parse(config["JWT:DurationInMinutes"])));

            return result;
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = config["JWT:Audience"],
                ValidateIssuer = true,
                ValidIssuer = config["JWT:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"])),
                ValidateLifetime = false // Ignore token expiration
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken is null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public async Task StoreTokenAsync(string userId, string tokenId, DateTime expirationTime)
        {
            var token = new TokenModel
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
