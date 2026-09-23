using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Identity.Application.Commands.MainMenus;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Application.Queries.MainMenus;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Main-menu endpoints. They live under api/Module because that is where the legacy
    /// API exposed them; the prefix is preserved and shared with ModulesController,
    /// which owns a disjoint set of action routes.
    /// </summary>
    [ApiController]
    [Route("api/Module")]
    [Authorize(Policy = Policies.IdentityAdmin)]
    public sealed class MainMenusController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MainMenusController(IMediator mediator) => _mediator = mediator;

        // ---------------------------------------------------------------- reads

        [HttpGet("GetAllMainMenu")]
        public async Task<ActionResult<PagedResult<MainMenuDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetMainMenusQuery(page, pageSize, search, null), ct));

        [HttpGet("GetAllActiveMenu")]
        public async Task<ActionResult<PagedResult<MainMenuDto>>> GetActive(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetMainMenusQuery(page, pageSize, search, true), ct));

        [HttpGet("GetAllInActiveMenu")]
        public async Task<ActionResult<PagedResult<MainMenuDto>>> GetInactive(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetMainMenusQuery(page, pageSize, search, false), ct));

        [HttpGet("GetMenuByStatus/{status}")]
        public async Task<ActionResult<PagedResult<MainMenuDto>>> GetByStatus(
            bool status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetMainMenusQuery(page, pageSize, search, status), ct));

        [HttpGet("GetMenuById/{id}")]
        public async Task<ActionResult<MainMenuDto>> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetMainMenuByIdQuery(id), ct);
            return result.Succeeded ? Ok(result.Value) : NotFound(new { message = result.Error });
        }

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewMenu")]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateMainMenuCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Succeeded ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [HttpPut("UpdateMenu/{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateMainMenuCommand command, CancellationToken ct)
        {
            if (id != command.MenuId)
                return BadRequest(new { message = "Route id and body id must match." });

            var result = await _mediator.Send(command, ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }

        [HttpPut("InActiveMenu/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SetMainMenuActiveCommand(id, false), ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }

        [HttpPut("ActivateMainMenu/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SetMainMenuActiveCommand(id, true), ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }
    }
}
