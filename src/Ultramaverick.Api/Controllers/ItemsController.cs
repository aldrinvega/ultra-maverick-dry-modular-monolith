using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Catalog.Application.Commands.Items;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Application.Queries.Items;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Raw material (item) endpoints. They live under api/RawMaterial because that is where the legacy API
    /// exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/RawMaterial")]
    [Authorize(Policy = Policies.Catalog)]
    public sealed class ItemsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ItemsController(IMediator mediator) => _mediator = mediator;

        // ---------------------------------------------------------------- reads

        [HttpGet("GetAllRawMaterialsWithPagination")]
        public async Task<ActionResult<PagedResult<ItemDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? status = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetItemsQuery(page, pageSize, search, status), ct));

        [HttpGet("GetRawMaterialById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetItemByIdQuery(id), ct)).ToOkOrNotFound();

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewRawMaterial")]
        public async Task<IActionResult> Create([FromBody] CreateItemCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToOk();

        [HttpPut("UpdateRawMaterial/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateItemCommand command, CancellationToken ct)
            => id != command.ItemId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("InActiveRawMaterial/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetItemActiveCommand(id, false), ct)).ToNoContent();

        [HttpPut("ActivateRawMaterial/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetItemActiveCommand(id, true), ct)).ToNoContent();
    }
}