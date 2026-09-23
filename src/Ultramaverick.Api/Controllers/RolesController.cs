using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
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
    }
}
