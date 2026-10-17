using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Infrastructure.Security;

namespace Ultramaverick.Catalog.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCatalogInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            _ = configuration;

            // Catalog resolves its own caller; do not rely on another module registering this.
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CatalogCurrentUser>();

            return services;
        }
    }
}