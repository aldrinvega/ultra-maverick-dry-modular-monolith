using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Catalog.Application.Commands.Uoms;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Application.Queries.Uoms;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Unit of measure endpoints. They live under api/Uom because that is where the legacy API
    /// exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/Uom")]
    [Authorize(Policy = Policies.Catalog)]
    public sealed class UomsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UomsController(IMediator mediator) => _mediator = mediator;

        // ---------------------------------------------------------------- reads

        [HttpGet("GetAllUomsWithPagination")]
        public async Task<ActionResult<PagedResult<UomDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? status = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetUomsQuery(page, pageSize, search, status), ct));

        [HttpGet("GetUomById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetUomByIdQuery(id), ct)).ToOkOrNotFound();

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewUom")]
        public async Task<IActionResult> Create([FromBody] CreateUomCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToOk();

        [HttpPut("UpdateUom/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUomCommand command, CancellationToken ct)
            => id != command.UomId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("InActiveUom/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetUomActiveCommand(id, false), ct)).ToNoContent();

        [HttpPut("ActivateUom/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetUomActiveCommand(id, true), ct)).ToNoContent();
    }
}