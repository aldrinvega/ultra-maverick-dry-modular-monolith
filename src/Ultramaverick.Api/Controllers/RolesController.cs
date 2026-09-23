using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
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

        /// <summary>All roles (active and inactive), paged.</summary>
        [HttpGet("GetAllRoles")]
        public async Task<ActionResult<PagedResult<RoleDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetRolesQuery(page, pageSize, search, null), ct));

        /// <summary>Roles filtered by active status, paged.</summary>
        [HttpGet("GetAllRolesWithPagination/{status}")]
        public async Task<ActionResult<PagedResult<RoleDto>>> GetPaged(
            bool status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetRolesQuery(page, pageSize, search, status), ct));

        [HttpGet("GetbyId/{id}")]
        public async Task<ActionResult<RoleDto>> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetRoleByIdQuery(id), ct);
            return result.Succeeded ? Ok(result.Value) : NotFound(new { message = result.Error });
        }

        /// <summary>The modules currently granted to a role.</summary>
        [HttpGet("GetRoleModules/{id}")]
        public async Task<ActionResult<IReadOnlyList<ModuleDto>>> GetModules(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetRoleModulesQuery(id), ct);
            return result.Succeeded ? Ok(result.Value) : NotFound(new { message = result.Error });
        }

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewRole")]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateRoleCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Succeeded ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [HttpPut("UpdateRole/{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateRoleCommand command, CancellationToken ct)
        {
            if (id != command.RoleId)
                return BadRequest(new { message = "Route id and body id must match." });

            var result = await _mediator.Send(command, ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }

        [HttpPut("InActiveRole/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SetRoleActiveCommand(id, false), ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }

        [HttpPut("ActivateRole/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SetRoleActiveCommand(id, true), ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }

        /// <summary>Replaces a role's module grants with the supplied set (empty clears them).</summary>
        [HttpPost("TagandModules")]
        public async Task<IActionResult> Tag([FromBody] TagModulesCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }

        /// <summary>Deactivates the listed grants, leaving other grants untouched.</summary>
        [HttpPut("UntagModule")]
        public async Task<IActionResult> Untag([FromBody] UntagModulesCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }
    }
}
