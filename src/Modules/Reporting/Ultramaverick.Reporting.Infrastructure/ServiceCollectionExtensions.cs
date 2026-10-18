using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Reporting.Infrastructure.Events;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReportingInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            _ = configuration;

            // Consume the five published events through the shared dispatcher.
            services.AddScoped<IIntegrationEventHandler, StockReceivedBridge>();
            services.AddScoped<IIntegrationEventHandler, OrderCreatedBridge>();
            services.AddScoped<IIntegrationEventHandler, OrderConfirmedBridge>();
            services.AddScoped<IIntegrationEventHandler, OrderCancelledBridge>();
            services.AddScoped<IIntegrationEventHandler, QcReceiptCompletedBridge>();

            return services;
        }
    }
}
