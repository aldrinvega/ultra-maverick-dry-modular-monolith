using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Infrastructure.Events;
using Ultramaverick.Quality.Infrastructure.Security;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Quality.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQualityInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            _ = configuration;

            // Quality resolves its own caller; do not rely on another module registering this.
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, QualityCurrentUser>();

            // Consume Procurement's PurchaseOrderReceived through the shared dispatcher.
            services.AddScoped<IIntegrationEventHandler, PurchaseOrderReceivedBridge>();

            return services;
        }
    }
}
