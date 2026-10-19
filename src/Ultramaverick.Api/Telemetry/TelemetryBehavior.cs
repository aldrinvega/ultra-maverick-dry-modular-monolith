using System.Diagnostics;
using MediatR;

namespace Ultramaverick.Api.Telemetry
{
    /// <summary>
    /// Opens an activity for every request, tagged with the owning module and whether the
    /// handler reported success, so module work is visible in traces.
    /// </summary>
    public sealed class TelemetryBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private static readonly ActivitySource Source = new("Ultramaverick.Modules");

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var module = ResolveModule(typeof(TRequest).Namespace);

            using var activity = Source.StartActivity($"{module}.{typeof(TRequest).Name}");
            activity?.SetTag("module", module);
            activity?.SetTag("request", typeof(TRequest).Name);

            try
            {
                var response = await next();

                if (response is not null
                    && response.GetType().GetProperty("Succeeded")?.GetValue(response) is bool succeeded)
                {
                    activity?.SetTag("result.succeeded", succeeded);
                }

                return response;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.GetType().Name);
                throw;
            }
        }

        private static string ResolveModule(string? ns)
        {
            // Ultramaverick.<Module>.Application... -> <Module>
            var parts = (ns ?? string.Empty).Split('.');
            return parts.Length > 1 && parts[0] == "Ultramaverick" ? parts[1] : "Shared";
        }
    }
}
