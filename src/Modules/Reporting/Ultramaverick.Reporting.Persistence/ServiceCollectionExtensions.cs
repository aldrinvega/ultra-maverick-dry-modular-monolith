using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Persistence.Repositories;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReportingPersistence(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<ReportingDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IReportingUnitOfWork, ReportingUnitOfWork>();
            services.AddScoped<IInventoryPositionReader, InventoryPositionReader>();
            services.AddScoped<IProcessedEventStore, ReportingProcessedEventStore>();

            return services;
        }
    }
}
