using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Quality.Application.Commands.QcReceipts;
using Ultramaverick.Quality.Application.Models;
using Ultramaverick.Quality.Application.Queries.QcReceipts;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// QC-receipt endpoints. They live under api/Receiving because that is where the legacy
    /// API exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/Receiving")]
    [Authorize(Policy = Policies.Quality)]
    public sealed class QcReceiptsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QcReceiptsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetQcReceiptsWithPagination")]
        public async Task<ActionResult<PagedResult<QcReceiptDto>>> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null, CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetQcReceiptsQuery(page, pageSize, search), ct));

        [HttpPut("Reject/{id:int}")]
        public async Task<IActionResult> Reject(
            int id, [FromBody] RejectQcReceiptCommand cmd, CancellationToken ct)
        {
            if (id != cmd.QcReceiptId)
                return BadRequest(new ApiResponse(400, "Route id does not match payload id."));

            return (await _mediator.Send(cmd, ct)).ToNoContent();
        }
    }

    /// <summary>
    /// Maps Quality application results to HTTP responses. Quality owns its Result type, so the
    /// API adapts it here rather than reusing another module's mapping.
    /// </summary>
    internal static class QualityResultExtensions
    {
        public static IActionResult ToNoContent(this Result result)
            => result.Succeeded
                ? new NoContentResult()
                : new BadRequestObjectResult(new ApiResponse(400, result.Error));
    }
}
