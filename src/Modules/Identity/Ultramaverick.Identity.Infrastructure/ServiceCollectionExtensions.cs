using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Infrastructure.Security;

namespace Ultramaverick.Identity.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<Options.JwtOptions>()
                .Bind(configuration.GetSection(Options.JwtOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Jwt:Key is required")
                .Validate(o => Encoding.UTF8.GetByteCount(o.Key) >= 32, "Jwt:Key must be at least 32 bytes")
                .Validate(o => o.AccessTokenMinutes > 0, "Jwt:AccessTokenMinutes must be positive")
                .Validate(o => o.RefreshTokenDays > 0, "Jwt:RefreshTokenDays must be positive")
                .ValidateOnStart();

            services.AddHttpContextAccessor();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            return services;
        }
    }
}
