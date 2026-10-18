using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Persistence.Lookups;
using Ultramaverick.Quality.Persistence.Repositories;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Quality.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQualityPersistence(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<QualityDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IQualityUnitOfWork, QualityUnitOfWork>();
            services.AddScoped<IPurchaseOrderLineLookup, PurchaseOrderLineLookup>();

            services.AddScoped<IOutboxStore, QualityOutboxStore>();
            services.AddScoped<IProcessedEventStore, QualityProcessedEventStore>();

            return services;
        }
    }
}
