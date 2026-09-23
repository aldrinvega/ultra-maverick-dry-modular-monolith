using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Application.Queries.Departments;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Department read endpoints. They live under api/User because that is where the
    /// legacy API exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/User")]
    [Authorize(Policy = Policies.IdentityAdmin)]
    public sealed class DepartmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DepartmentsController(IMediator mediator) => _mediator = mediator;

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
    }
}
