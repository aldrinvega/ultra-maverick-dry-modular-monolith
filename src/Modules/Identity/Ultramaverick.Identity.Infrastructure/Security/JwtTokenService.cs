using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Domain.Entities;
using Ultramaverick.Identity.Infrastructure.Options;

namespace Ultramaverick.Identity.Infrastructure.Security
{
    public sealed class JwtTokenService : ITokenService
    {
        public const string ModuleClaimType = "modules";

        private readonly JwtOptions _options;
        private readonly JwtSecurityTokenHandler _handler = new();
        private readonly SymmetricSecurityKey _key;
        
        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        }
        public AccessToken CreateAccessToken(User user, IReadOnlyCollection<string> moduleNames)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_options.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Role, user.RoleId.ToString())
            };

            foreach (var module in moduleNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ModuleClaimType, module));
            }

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
            );

            return new AccessToken(_handler.WriteToken(token), expires);
        }

        public string CreateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public int RefreshTokenDays => _options.RefreshTokenDays;
    }
}
