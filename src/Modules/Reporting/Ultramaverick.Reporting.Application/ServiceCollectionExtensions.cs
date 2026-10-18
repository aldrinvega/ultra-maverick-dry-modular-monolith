using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ultramaverick.Reporting.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportingApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        return services;
    }
}
