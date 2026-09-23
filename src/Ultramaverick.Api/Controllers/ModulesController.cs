using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Identity.Application.Commands.Modules;
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

        // ---------------------------------------------------------------- reads

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
        public async Task<ActionResult<ModuleDto>> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetModuleByIdQuery(id), ct);
            return result.Succeeded ? Ok(result.Value) : NotFound(new { message = result.Error });
        }

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewModule")]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateModuleCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Succeeded ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [HttpPut("UpdateModule/{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateModuleCommand command, CancellationToken ct)
        {
            if (id != command.ModuleId)
                return BadRequest(new { message = "Route id and body id must match." });

            var result = await _mediator.Send(command, ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }

        [HttpPut("InActiveModule/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SetModuleActiveCommand(id, false), ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }

        [HttpPut("ActivateModule/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SetModuleActiveCommand(id, true), ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }
    }
}
