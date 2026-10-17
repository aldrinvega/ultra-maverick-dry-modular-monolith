using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Catalog.Application.Commands.Customers;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Application.Queries.Customers;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Customer endpoints. They live under api/Customer because that is where the legacy API
    /// exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/Customer")]
    [Authorize(Policy = Policies.Catalog)]
    public sealed class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator) => _mediator = mediator;

        // ---------------------------------------------------------------- reads

        [HttpGet("GetAllCustomersWithPagination")]
        public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? status = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetCustomersQuery(page, pageSize, search, status), ct));

        [HttpGet("GetCustomerById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetCustomerByIdQuery(id), ct)).ToOkOrNotFound();

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewCustomer")]
        public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToOk();

        [HttpPut("UpdateCustomer/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerCommand command, CancellationToken ct)
            => id != command.CustomerId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("InActiveCustomer/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetCustomerActiveCommand(id, false), ct)).ToNoContent();

        [HttpPut("ActivateCustomer/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetCustomerActiveCommand(id, true), ct)).ToNoContent();
    }
}