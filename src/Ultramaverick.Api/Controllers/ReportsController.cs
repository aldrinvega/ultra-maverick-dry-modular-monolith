using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Reporting.Application.Queries.InventoryPositions;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>Reporting dashboards, served from the module's projected read models.</summary>
    /// <remarks>
    /// Projections are eventually consistent. Data the caller has just written must be read from
    /// the owning module's endpoint, not from Reporting.
    /// </remarks>
    [ApiController]
    [Route("api/Reports")]
    [Authorize(Policy = Policies.Reporting)]
    public sealed class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("InventoryPosition")]
        public async Task<IActionResult> Inventory(
            [FromQuery] int? warehouseId, CancellationToken ct)
            => (await _mediator.Send(new GetInventoryPositionQuery(warehouseId), ct)).ToOk();
    }
}
