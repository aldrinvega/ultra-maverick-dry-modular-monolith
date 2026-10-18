using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Reporting.Application.Models;

namespace Ultramaverick.Api.Errors
{
    /// <summary>Maps Reporting application results to HTTP responses.</summary>
    public static class ReportingResultExtensions
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
