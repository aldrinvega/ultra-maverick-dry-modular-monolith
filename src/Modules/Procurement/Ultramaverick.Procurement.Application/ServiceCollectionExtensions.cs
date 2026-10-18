using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Procurement.Application.Behaviors;

namespace Ultramaverick.Procurement.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProcurementApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        services.AddScoped<Ultramaverick.Procurement.Application.Import.IImportReader, Ultramaverick.Procurement.Application.Import.XlsxImportReader>();

        // Run validators before handlers.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
