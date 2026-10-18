using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Infrastructure.Events;
using Ultramaverick.Warehouse.Infrastructure.Security;

namespace Ultramaverick.Warehouse.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWarehouseInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            _ = configuration;

            // Warehouse resolves its own caller; do not rely on another module registering this.
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, WarehouseCurrentUser>();

            // Consume Quality's QcReceiptCompleted through the shared dispatcher.
            services.AddScoped<IIntegrationEventHandler, QcReceiptCompletedBridge>();

            return services;
        }
    }
}
