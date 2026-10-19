using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Procurement.Application.Commands.PurchaseOrders;
using Ultramaverick.Procurement.Application.Models;
using Ultramaverick.Procurement.Application.Queries.PurchaseOrders;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Purchase-order endpoints. They live under api/Ordering because that is where the
    /// legacy API exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/Ordering")]
    [Authorize(Policy = Policies.Procurement)]
    public sealed class PurchaseOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PurchaseOrdersController(IMediator mediator) => _mediator = mediator;

        [HttpPost("AddPurchaseOrder")]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderCommand cmd, CancellationToken ct)
            => (await _mediator.Send(cmd, ct)).ToOk();

        [HttpGet("GetPurchaseOrders")]
        public async Task<ActionResult<PagedResult<PurchaseOrderDto>>> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null, CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetPurchaseOrdersQuery(page, pageSize, search), ct));

        [HttpGet("GetPurchaseOrderById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPurchaseOrderByIdQuery(id), ct);
            return result.Succeeded ? Ok(result.Value) : NotFound(new ApiResponse(404, result.Error));
        }
    }

    /// <summary>
    /// Maps Procurement application results to HTTP responses. Procurement owns its Result
    /// type, so the API adapts it here rather than reusing another module's mapping.
    /// </summary>
    internal static class ProcurementResultExtensions
    {
        public static IActionResult ToOk<T>(this Result<T> result)
            => result.Succeeded
                ? new OkObjectResult(result.Value)
                : new BadRequestObjectResult(new ApiResponse(400, result.Error));
    }
}
