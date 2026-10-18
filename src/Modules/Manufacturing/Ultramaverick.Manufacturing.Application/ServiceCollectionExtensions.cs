using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Manufacturing.Application.Behaviors;

namespace Ultramaverick.Manufacturing.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddManufacturingApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        // Run validators before handlers.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
