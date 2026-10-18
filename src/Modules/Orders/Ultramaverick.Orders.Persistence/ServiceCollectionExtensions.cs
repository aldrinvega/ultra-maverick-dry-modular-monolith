using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Orders.Application.Abstractions;
using Ultramaverick.Orders.Persistence.Lookups;
using Ultramaverick.Orders.Persistence.Repositories;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Orders.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrdersPersistence(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<OrdersDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IOrdersUnitOfWork, OrdersUnitOfWork>();
            services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
            services.AddScoped<ICustomerLookup, CustomerLookup>();
            services.AddScoped<ICancelledOrderReader, CancelledOrderReader>();

            services.AddScoped<IOutboxStore, OrdersOutboxStore>();
            services.AddScoped<IProcessedEventStore, OrdersProcessedEventStore>();

            return services;
        }
    }
}
