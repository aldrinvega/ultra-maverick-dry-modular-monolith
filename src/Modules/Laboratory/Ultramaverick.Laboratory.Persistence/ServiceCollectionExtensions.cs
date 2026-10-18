using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Laboratory.Application.Abstractions;
using Ultramaverick.Laboratory.Persistence.Repositories;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Laboratory.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLaboratoryPersistence(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<LaboratoryDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<ILaboratoryUnitOfWork, LaboratoryUnitOfWork>();

            services.AddScoped<IOutboxStore, LaboratoryOutboxStore>();
            services.AddScoped<IProcessedEventStore, LaboratoryProcessedEventStore>();

            return services;
        }
    }
}
