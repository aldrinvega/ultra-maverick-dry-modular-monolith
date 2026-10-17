using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Api.Errors
{
    /// <summary>
    /// Maps Catalog application results to the same HTTP responses as the Identity module's
    /// results. Each module owns a Result type, so the API adapts each one.
    /// </summary>
    public static class CatalogResultExtensions
    {
        public static IActionResult ToOk<T>(this Result<T> result)
            => result.Succeeded
                ? new OkObjectResult(result.Value)
                : new BadRequestObjectResult(new ApiResponse(400, result.Error));

        public static IActionResult ToOkOrNotFound<T>(this Result<T> result)
            => result.Succeeded
                ? new OkObjectResult(result.Value)
                : new NotFoundObjectResult(new ApiResponse(404, result.Error));

        public static IActionResult ToNoContent(this Result result)
            => result.Succeeded
                ? new NoContentResult()
                : new BadRequestObjectResult(new ApiResponse(400, result.Error));
    }
}