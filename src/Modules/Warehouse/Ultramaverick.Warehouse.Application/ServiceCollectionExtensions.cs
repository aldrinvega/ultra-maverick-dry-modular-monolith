using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Warehouse.Application.Behaviors;
using Ultramaverick.Warehouse.Application.Services;

namespace Ultramaverick.Warehouse.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWarehouseApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        // Run validators before handlers.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // The ledger is the only writer of on-hand; resolved by the receipt/adjustment handlers.
        services.AddScoped<InventoryLedger>();

        return services;
    }
}
