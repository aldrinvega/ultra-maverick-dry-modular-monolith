using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Orders.Application.Models;

namespace Ultramaverick.Api.Errors
{
    /// <summary>
    /// Maps Orders application results to HTTP responses. Orders owns its Result type, so
    /// the API adapts it here rather than reusing another module's mapping.
    /// </summary>
    public static class OrdersResultExtensions
    {
        public static IActionResult ToOk<T>(this Result<T> result)
            => result.Succeeded
                ? new OkObjectResult(result.Value)
                : new BadRequestObjectResult(new ApiResponse(400, result.Error));

        public static IActionResult ToNoContent(this Result result)
            => result.Succeeded
                ? new NoContentResult()
                : new BadRequestObjectResult(new ApiResponse(400, result.Error));
    }
}
