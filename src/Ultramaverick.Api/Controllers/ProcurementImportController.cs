using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Procurement.Application.Commands.PurchaseOrders;
using Ultramaverick.Procurement.Application.Import;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Procurement workbook uploads. The legacy API exposed UploadPOSummaryReport on its
    /// ImportController; the route prefix now belongs to the module that owns the data.
    /// </summary>
    [ApiController]
    [Route("api/Procurement")]
    [Authorize(Policy = Policies.Procurement)]
    public sealed class ProcurementImportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProcurementImportController(IMediator mediator) => _mediator = mediator;

        [HttpPost("UploadPOSummaryReport")]
        public async Task<IActionResult> UploadPosSummaryReport(
            IFormFile? file, CancellationToken ct)
        {
            var rejection = ImportFileRules.Reject(file?.FileName, file?.ContentType, file?.Length ?? 0);
            if (rejection is not null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, rejection));

            using var stream = file!.OpenReadStream();

            var result = await _mediator.Send(new ImportPurchaseOrdersCommand(stream), ct);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, result.Error));

            var report = result.Value!;
            return report.Failed.Count > 0
                ? UnprocessableEntity(report)
                : Ok(report);
        }
    }
}
