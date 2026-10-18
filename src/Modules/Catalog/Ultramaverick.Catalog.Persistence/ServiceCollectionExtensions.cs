using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Persistence.Repositories;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Catalog.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCatalogPersistence(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<ICatalogUnitOfWork, CatalogUnitOfWork>();

            services.AddScoped<IOutboxStore, OutboxStore>();

            return services;
        }
    }
}