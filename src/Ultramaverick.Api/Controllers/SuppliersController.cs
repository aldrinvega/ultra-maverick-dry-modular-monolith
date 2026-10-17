using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Catalog.Application.Commands.Suppliers;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Application.Queries.Suppliers;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Supplier endpoints. They live under api/Supplier because that is where the legacy API
    /// exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/Supplier")]
    [Authorize(Policy = Policies.Catalog)]
    public sealed class SuppliersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SuppliersController(IMediator mediator) => _mediator = mediator;

        // ---------------------------------------------------------------- reads

        [HttpGet("GetAllSuppliersWithPagination")]
        public async Task<ActionResult<PagedResult<SupplierDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? status = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetSuppliersQuery(page, pageSize, search, status), ct));

        [HttpGet("GetSupplierById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetSupplierByIdQuery(id), ct)).ToOkOrNotFound();

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewSupplier")]
        public async Task<IActionResult> Create([FromBody] CreateSupplierCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToOk();

        [HttpPut("UpdateSupplier/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierCommand command, CancellationToken ct)
            => id != command.SupplierId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("InActiveSupplier/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetSupplierActiveCommand(id, false), ct)).ToNoContent();

        [HttpPut("ActivateSupplier/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetSupplierActiveCommand(id, true), ct)).ToNoContent();
    }
}