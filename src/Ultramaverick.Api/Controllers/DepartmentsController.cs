using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Identity.Application.Commands.Departments;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Application.Queries.Departments;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Department endpoints. They live under api/User because that is where the
    /// legacy API exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/User")]
    [Authorize(Policy = Policies.IdentityAdmin)]
    public sealed class DepartmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DepartmentsController(IMediator mediator) => _mediator = mediator;

        // ---------------------------------------------------------------- reads

        [HttpGet("GetAllDepartments")]
        public async Task<ActionResult<PagedResult<DepartmentDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetDepartmentsQuery(page, pageSize, search, null), ct));

        [HttpGet("GetAllActiveDepartment")]
        public async Task<ActionResult<PagedResult<DepartmentDto>>> GetActive(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetDepartmentsQuery(page, pageSize, search, true), ct));

        [HttpGet("GetAllInActiveDepartment")]
        public async Task<ActionResult<PagedResult<DepartmentDto>>> GetInactive(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetDepartmentsQuery(page, pageSize, search, false), ct));

        [HttpGet("GetDepartmentByStatus/{status}")]
        public async Task<ActionResult<PagedResult<DepartmentDto>>> GetByStatus(
            bool status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetDepartmentsQuery(page, pageSize, search, status), ct));

        [HttpGet("GetAllDepartmentById/{id}")]
        public async Task<ActionResult<DepartmentDto>> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetDepartmentByIdQuery(id), ct);
            return result.Succeeded ? Ok(result.Value) : NotFound(new { message = result.Error });
        }

        // --------------------------------------------------------------- writes

        [HttpPost("AddNewDepartment")]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateDepartmentCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Succeeded ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [HttpPut("UpdateDepartmentInfo/{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateDepartmentCommand command, CancellationToken ct)
        {
            if (id != command.DepartmentId)
                return BadRequest(new { message = "Route id and body id must match." });

            var result = await _mediator.Send(command, ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }

        [HttpPut("InActiveDepartment/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SetDepartmentActiveCommand(id, false), ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }

        [HttpPut("ActivateDepartment/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SetDepartmentActiveCommand(id, true), ct);
            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }
    }
}
