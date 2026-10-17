using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Catalog.Application.Commands.Reasons;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Application.Queries.Reasons;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Reason endpoints. They live under api/Reason because that is where the legacy API
    /// exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/Reason")]
    [Authorize(Policy = Policies.Catalog)]
    public sealed class ReasonsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReasonsController(IMediator mediator) => _mediator = mediator;

        // ---------------------------------------------------------------- reads

        [HttpGet("GetAllReasonsWithPagination")]
        public async Task<ActionResult<PagedResult<ReasonDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? status = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetReasonsQuery(page, pageSize, search, status), ct));

        [HttpGet("GetReasonById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetReasonByIdQuery(id), ct)).ToOkOrNotFound();

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewReason")]
        public async Task<IActionResult> Create([FromBody] CreateReasonCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToOk();

        [HttpPut("UpdateReason/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReasonCommand command, CancellationToken ct)
            => id != command.ReasonId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("InActiveReason/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetReasonActiveCommand(id, false), ct)).ToNoContent();

        [HttpPut("ActivateReason/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetReasonActiveCommand(id, true), ct)).ToNoContent();
    }
}