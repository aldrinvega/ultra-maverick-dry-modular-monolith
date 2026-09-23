using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Infrastructure.BackgroundServices;
using Ultramaverick.Identity.Infrastructure.Options;
using Ultramaverick.Identity.Infrastructure.Security;

namespace Ultramaverick.Identity.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Jwt:Key is required")
                .Validate(o => Encoding.UTF8.GetByteCount(o.Key) >= 32, "Jwt:Key must be at least 32 bytes")
                .Validate(o => o.AccessTokenMinutes > 0, "Jwt:AccessTokenMinutes must be positive")
                .Validate(o => o.RefreshTokenDays > 0, "Jwt:RefreshTokenDays must be positive")
                .ValidateOnStart();

            services.AddHttpContextAccessor();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            services.AddHostedService<RefreshTokenPrunerService>();

            return services;
        }
    }
}
