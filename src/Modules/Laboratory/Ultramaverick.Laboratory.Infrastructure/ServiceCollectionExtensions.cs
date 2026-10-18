using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Laboratory.Application.Abstractions;
using Ultramaverick.Laboratory.Infrastructure.Security;

namespace Ultramaverick.Laboratory.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLaboratoryInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            _ = configuration;

            // Laboratory resolves its own caller; do not rely on another module registering this.
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, LaboratoryCurrentUser>();

            return services;
        }
    }
}
