using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Catalog.Application.Commands.Formulas;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Application.Queries.Formulas;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Transformation (formula) endpoints. The route prefix is preserved from the legacy
    /// TransformationController; a formula pairs an item with a versioned bill of materials.
    /// </summary>
    [ApiController]
    [Route("api/Transformation")]
    [Authorize(Policy = Policies.Catalog)]
    public sealed class FormulasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FormulasController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetAllFormulasWithPagination")]
        public async Task<ActionResult<PagedResult<FormulaDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? itemId = null,
            [FromQuery] bool? status = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetFormulasQuery(page, pageSize, itemId, status), ct));

        [HttpGet("GetFormulaById/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetFormulaByIdQuery(id), ct)).ToOkOrNotFound();

        [HttpPost("AddNewFormula")]
        public async Task<IActionResult> Create([FromBody] CreateFormulaCommand command, CancellationToken ct)
            => (await _mediator.Send(command, ct)).ToOk();

        [HttpPost("AddFormulaRequirement/{id}")]
        public async Task<IActionResult> AddRequirement(
            int id, [FromBody] AddFormulaRequirementCommand command, CancellationToken ct)
            => id != command.FormulaId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(command, ct)).ToNoContent();

        [HttpPut("InActiveFormula/{id}")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetFormulaActiveCommand(id, false), ct)).ToNoContent();

        [HttpPut("ActivateFormula/{id}")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => (await _mediator.Send(new SetFormulaActiveCommand(id, true), ct)).ToNoContent();
    }
}