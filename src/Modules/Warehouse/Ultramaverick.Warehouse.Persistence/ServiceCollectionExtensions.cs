using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Persistence.Repositories;

namespace Ultramaverick.Warehouse.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWarehousePersistence(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<WarehouseDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IWarehouseUnitOfWork, WarehouseUnitOfWork>();
            services.AddScoped<IWarehouseSelector, WarehouseSelector>();

            services.AddScoped<IOutboxStore, WarehouseOutboxStore>();
            services.AddScoped<IProcessedEventStore, WarehouseProcessedEventStore>();

            return services;
        }
    }
}
