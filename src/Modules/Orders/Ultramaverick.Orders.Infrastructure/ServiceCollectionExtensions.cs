using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Infrastructure.Security;

namespace Ultramaverick.Orders.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrdersInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            _ = configuration;

            // Orders resolves its own caller; do not rely on another module registering this.
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, OrdersCurrentUser>();

            return services;
        }
    }
}
