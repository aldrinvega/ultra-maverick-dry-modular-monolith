using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Application.Queries.Modules;

namespace Ultramaverick.Api.Controllers
{
    [ApiController]
    [Route("api/Module")]
    [Authorize(Policy = Policies.IdentityAdmin)]
    public sealed class ModulesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ModulesController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetAllModules")]
        public async Task<ActionResult<PagedResult<ModuleDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetModulesQuery(page, pageSize, search, null), ct));

        [HttpGet("GetAllModulesWithPagination/{status}")]
        public async Task<ActionResult<PagedResult<ModuleDto>>> GetPaged(
            bool status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetModulesQuery(page, pageSize, search, status), ct));

        [HttpGet("GetModuleByStatus/{status}")]
        public async Task<ActionResult<PagedResult<ModuleDto>>> GetByStatus(
            bool status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetModulesQuery(page, pageSize, search, status), ct));

        [HttpGet("GetAllActiveModules")]
        public async Task<ActionResult<PagedResult<ModuleDto>>> GetActive(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetModulesQuery(page, pageSize, search, true), ct));

        [HttpGet("GetAllInActiveModules")]
        public async Task<ActionResult<PagedResult<ModuleDto>>> GetInactive(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetModulesQuery(page, pageSize, search, false), ct));

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetModuleByIdQuery(id), ct)).ToOkOrNotFound();
    }
}
