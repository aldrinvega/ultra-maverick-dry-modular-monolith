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
    /// Account title endpoints. They live under api/AccountTitle because that is where the legacy API
    /// exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/AccountTitle")]
    [Authorize(Policy = Policies.Catalog)]
    public sealed class AccountTitlesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountTitlesController(IMediator mediator) => _mediator = mediator;

        // ---------------------------------------------------------------- reads

        [HttpGet("GetAllAccountTitlesWithPagination")]
        public async Task<ActionResult<PagedResult<LookupDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? status = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetAccountTitlesQuery(page, pageSize, search, status), ct));

        [HttpGet("GetAccountTitleById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetAccountTitleByIdQuery(id), ct)).ToOkOrNotFound();

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewAccountTitle")]
        public async Task<IActionResult> Create([FromBody] CreateAccountTitleCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToOk();

        [HttpPut("UpdateAccountTitle/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountTitleCommand command, CancellationToken ct)
            => id != command.AccountTitleId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("InActiveAccountTitle/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetAccountTitleActiveCommand(id, false), ct)).ToNoContent();

        [HttpPut("ActivateAccountTitle/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetAccountTitleActiveCommand(id, true), ct)).ToNoContent();
    }
}