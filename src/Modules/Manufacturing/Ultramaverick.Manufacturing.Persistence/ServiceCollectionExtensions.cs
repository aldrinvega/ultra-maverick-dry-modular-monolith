using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Persistence.Repositories;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Manufacturing.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddManufacturingPersistence(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<ManufacturingDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IManufacturingUnitOfWork, ManufacturingUnitOfWork>();
            services.AddScoped<ITransformationRequestReader, TransformationRequestReader>();

            services.AddScoped<IOutboxStore, ManufacturingOutboxStore>();
            services.AddScoped<IProcessedEventStore, ManufacturingProcessedEventStore>();

            return services;
        }
    }
}

