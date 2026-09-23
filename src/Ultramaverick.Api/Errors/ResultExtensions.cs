using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Api.Errors
{
    /// <summary>
    /// Maps Application results to consistent HTTP responses:
    /// success carries the value (or 204), failure carries an ApiResponse
    /// with the operation's message.
    /// </summary>
    public static class ResultExtensions
    {
        public static IActionResult ToOk<T>(this Result<T> result)
            => result.Succeeded
                ? new OkObjectResult(result.Value)
                : new BadRequestObjectResult(new ApiResponse(400, result.Error));

        public static IActionResult ToOkOrNotFound<T>(this Result<T> result)
            => result.Succeeded
                ? new OkObjectResult(result.Value)
                : new NotFoundObjectResult(new ApiResponse(404, result.Error));

        public static IActionResult ToOkOrUnauthorized<T>(this Result<T> result)
            => result.Succeeded
                ? new OkObjectResult(result.Value)
                : new UnauthorizedObjectResult(new ApiResponse(401, result.Error));

        public static IActionResult ToNoContent(this Result result)
            => result.Succeeded
                ? new NoContentResult()
                : new BadRequestObjectResult(new ApiResponse(400, result.Error));

        public static IActionResult ToUnauthorized(this Result result)
            => result.Succeeded
                ? new OkResult()
                : new UnauthorizedObjectResult(new ApiResponse(401, result.Error));

        public static IActionResult BadRequestError(string message)
            => new BadRequestObjectResult(new ApiResponse(400, message));
    }
}
