using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Catalog.Application.Commands.Formulas;
using Ultramaverick.Catalog.Application.Commands.Items;
using Ultramaverick.Catalog.Application.Import;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Catalog workbook uploads. The legacy API exposed UploadRawMaterialsReport and
    /// UploadFormulaReport on its ImportController; the routes now belong to the module that owns
    /// the data.
    /// </summary>
    [ApiController]
    [Route("api/Catalog")]
    [Authorize(Policy = Policies.Catalog)]
    public sealed class CatalogImportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CatalogImportController(IMediator mediator) => _mediator = mediator;

        [HttpPost("UploadRawMaterialsReport")]
        public Task<IActionResult> UploadRawMaterialsReport(IFormFile? file, CancellationToken ct)
            => ImportAsync(file, content => new ImportRawMaterialsCommand(content), ct);

        [HttpPost("UploadFormulaReport")]
        public Task<IActionResult> UploadFormulaReport(IFormFile? file, CancellationToken ct)
            => ImportAsync(file, content => new ImportFormulasCommand(content), ct);

        private async Task<IActionResult> ImportAsync<TCommand>(
            IFormFile? file,
            Func<Stream, TCommand> command,
            CancellationToken ct)
            where TCommand : IRequest<Result<ImportReport>>
        {
            var rejection = ImportFileRules.Reject(file?.FileName, file?.ContentType, file?.Length ?? 0);
            if (rejection is not null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, rejection));

            using var stream = file!.OpenReadStream();

            var result = await _mediator.Send(command(stream), ct);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, result.Error));

            var report = result.Value!;
            return report.Failed.Count > 0
                ? UnprocessableEntity(report)
                : Ok(report);
        }
    }
}
