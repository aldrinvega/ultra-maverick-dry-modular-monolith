using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Catalog.Application.Behaviors;
using Ultramaverick.Catalog.Application.Services;

namespace Ultramaverick.Catalog.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        services.AddScoped<Ultramaverick.Catalog.Application.Import.IImportReader, Ultramaverick.Catalog.Application.Import.XlsxImportReader>();
        services.AddScoped<Ultramaverick.Catalog.Application.Abstractions.IReferenceDataService, ReferenceDataService>();

        // Run validators before handlers.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}