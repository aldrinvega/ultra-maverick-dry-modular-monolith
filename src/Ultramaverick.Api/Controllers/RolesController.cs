using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Identity.Application.Commands.Roles;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Application.Queries.Roles;

namespace Ultramaverick.Api.Controllers
{
    [ApiController]
    [Route("api/Role")]
    [Authorize(Policy = Policies.IdentityAdmin)]
    public sealed class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator) => _mediator = mediator;

        // ---------------------------------------------------------------- reads

        [HttpGet("GetAllRoles")]
        public async Task<ActionResult<PagedResult<RoleDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetRolesQuery(page, pageSize, search, null), ct));

        [HttpGet("GetAllRolesWithPagination/{status}")]
        public async Task<ActionResult<PagedResult<RoleDto>>> GetPaged(
            bool status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetRolesQuery(page, pageSize, search, status), ct));

        [HttpGet("GetbyId/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetRoleByIdQuery(id), ct)).ToOkOrNotFound();

        [HttpGet("GetRoleModules/{id}")]
        public async Task<IActionResult> GetModules(int id, CancellationToken ct)
            => (await _mediator.Send(new GetRoleModulesQuery(id), ct)).ToOkOrNotFound();

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewRole")]
        public async Task<IActionResult> Create(
            [FromBody] CreateRoleCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToOk();

        [HttpPut("UpdateRole/{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateRoleCommand command, CancellationToken ct)
            => id != command.RoleId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("InActiveRole/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetRoleActiveCommand(id, false), ct)).ToNoContent();

        [HttpPut("ActivateRole/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetRoleActiveCommand(id, true), ct)).ToNoContent();

        [HttpPost("TagandModules")]
        public async Task<IActionResult> Tag([FromBody] TagModulesCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("UntagModule")]
        public async Task<IActionResult> Untag([FromBody] UntagModulesCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToNoContent();
    }
}
