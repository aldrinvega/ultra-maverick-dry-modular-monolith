using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Identity.Application.Commands.Users;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Application.Queries.Users;

namespace Ultramaverick.Api.Controllers
{
    [ApiController]
    [Route("api/User")]
    [Authorize(Policy = Policies.IdentityAdmin)]
    public sealed class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetAllUsersWithPagination/{status}")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetAll(
            bool status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetUsersQuery(page, pageSize, search, status), ct));

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetUserByIdQuery(id), ct)).ToOkOrNotFound();

        [HttpPost("AddNewUser")]
        public async Task<IActionResult> Create(
            [FromBody] CreateUserCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToOk();

        [HttpPut("UpdateUserInfo/{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateUserCommand command, CancellationToken ct)
            => id != command.UserId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("InActiveUser/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetUserActiveCommand(id, false), ct)).ToNoContent();

        [HttpPut("ActivateUser/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetUserActiveCommand(id, true), ct)).ToNoContent();
    }
}
