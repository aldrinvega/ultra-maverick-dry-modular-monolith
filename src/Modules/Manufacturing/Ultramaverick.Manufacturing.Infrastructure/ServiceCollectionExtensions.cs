using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Infrastructure.Security;

namespace Ultramaverick.Manufacturing.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddManufacturingInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            _ = configuration;

            // Manufacturing resolves its own caller; do not rely on another module registering this.
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, ManufacturingCurrentUser>();

            return services;
        }
    }
}
