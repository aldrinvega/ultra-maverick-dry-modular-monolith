using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Infrastructure.Security;

namespace Ultramaverick.Procurement.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProcurementInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            _ = configuration;

            // Procurement resolves its own caller; do not rely on another module registering this.
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, ProcurementCurrentUser>();

            return services;
        }
    }
}
