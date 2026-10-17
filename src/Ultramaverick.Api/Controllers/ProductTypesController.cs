using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Catalog.Application.Commands.Lookups;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Application.Queries.Lookups;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Product type endpoints. They live under api/ProductType because that is where the legacy API
    /// exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/ProductType")]
    [Authorize(Policy = Policies.Catalog)]
    public sealed class ProductTypesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductTypesController(IMediator mediator) => _mediator = mediator;

        // ---------------------------------------------------------------- reads

        [HttpGet("GetAllProductTypesWithPagination")]
        public async Task<ActionResult<PagedResult<LookupDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? status = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetProductTypesQuery(page, pageSize, search, status), ct));

        [HttpGet("GetProductTypeById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetProductTypeByIdQuery(id), ct)).ToOkOrNotFound();

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewProductType")]
        public async Task<IActionResult> Create([FromBody] CreateProductTypeCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToOk();

        [HttpPut("UpdateProductType/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductTypeCommand command, CancellationToken ct)
            => id != command.ProductTypeId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("InActiveProductType/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetProductTypeActiveCommand(id, false), ct)).ToNoContent();

        [HttpPut("ActivateProductType/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetProductTypeActiveCommand(id, true), ct)).ToNoContent();
    }
}