using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Catalog.Application.Commands.LotNames;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Application.Queries.LotNames;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Lot name endpoints. They live under api/Lot because that is where the legacy API
    /// exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/Lot")]
    [Authorize(Policy = Policies.Catalog)]
    public sealed class LotNamesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LotNamesController(IMediator mediator) => _mediator = mediator;

        // ---------------------------------------------------------------- reads

        [HttpGet("GetAllLotNamesWithPagination")]
        public async Task<ActionResult<PagedResult<LotNameDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? status = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetLotNamesQuery(page, pageSize, search, status), ct));

        [HttpGet("GetLotNameById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetLotNameByIdQuery(id), ct)).ToOkOrNotFound();

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewLotName")]
        public async Task<IActionResult> Create([FromBody] CreateLotNameCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToOk();

        [HttpPut("UpdateLotName/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLotNameCommand command, CancellationToken ct)
            => id != command.LotNameId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("InActiveLotName/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetLotNameActiveCommand(id, false), ct)).ToNoContent();

        [HttpPut("ActivateLotName/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetLotNameActiveCommand(id, true), ct)).ToNoContent();
    }
}