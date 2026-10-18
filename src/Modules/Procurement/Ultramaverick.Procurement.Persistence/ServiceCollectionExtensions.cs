using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Persistence.Lookups;
using Ultramaverick.Procurement.Persistence.Repositories;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Procurement.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProcurementPersistence(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<ProcurementDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IProcurementUnitOfWork, ProcurementUnitOfWork>();
            services.AddScoped<ISupplierLookup, SupplierLookup>();

            services.AddScoped<IOutboxStore, OutboxStore>();

            return services;
        }
    }
}
