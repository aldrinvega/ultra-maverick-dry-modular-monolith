using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AppValidationException = Ultramaverick.Identity.Application.Exceptions.ValidationException;

namespace Ultramaverick.Api.Errors
{
    /// <summary>
    /// Converts unhandled exceptions into the shared error shape. Validation failures
    /// become 400 with the validation shape; everything else becomes 500, with details
    /// included only outside production.
    /// </summary>
    public sealed class ExceptionMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppValidationException ex)
            {
                await WriteAsync(context, StatusCodes.Status400BadRequest,
                    new ApiValidationError(ex.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);

                var body = _env.IsDevelopment()
                    ? new ApiException(500, ex.Message, ex.StackTrace)
                    : new ApiResponse(500);

                await WriteAsync(context, StatusCodes.Status500InternalServerError, body);
            }
        }

        private static async Task WriteAsync(HttpContext context, int statusCode, object body)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
        }
    }
}
