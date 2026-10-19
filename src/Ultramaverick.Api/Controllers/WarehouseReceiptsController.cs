using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Warehouse.Application.Commands.Inventory;
using Ultramaverick.Warehouse.Application.Commands.WarehouseReceipts;
using Ultramaverick.Warehouse.Application.Commands.Warehouses;
using Ultramaverick.Warehouse.Application.Models;
using Ultramaverick.Warehouse.Application.Queries.Expiring;
using Ultramaverick.Warehouse.Application.Queries.Inventory;
using Ultramaverick.Warehouse.Application.Queries.Warehouses;
using Ultramaverick.Warehouse.Application.Queries.Miscellaneous;
using Ultramaverick.Warehouse.Application.Queries.WarehouseReceipts;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Warehouse endpoints. They live under api/WarehouseReceiving because that is where the
    /// legacy API exposed them; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/WarehouseReceiving")]
    [Authorize(Policy = Policies.Warehouse)]
    public sealed class WarehouseReceiptsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WarehouseReceiptsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetWarehouseReceiptsWithPagination")]
        public async Task<ActionResult<PagedResult<WarehouseReceiptDto>>> GetWarehouseReceiptsWithPagination(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetWarehouseReceiptsQuery(page, pageSize), ct));

        [HttpGet("GetWarehouseReceiptById/{id}")]
        public async Task<IActionResult> GetWarehouseReceiptById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetWarehouseReceiptByIdQuery(id), ct)).ToOk();

        [HttpGet("GetMiscellaneousReceiptsWithPagination")]
        public async Task<ActionResult<PagedResult<MiscellaneousReceiptDto>>> GetMiscellaneousReceipts(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetMiscellaneousReceiptsQuery(page, pageSize), ct));

        [HttpGet("GetMiscellaneousIssuesWithPagination")]
        public async Task<ActionResult<PagedResult<MiscellaneousIssueDto>>> GetMiscellaneousIssues(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetMiscellaneousIssuesQuery(page, pageSize), ct));
        [HttpGet("GetExpiringReceipts")]
        public async Task<ActionResult<PagedResult<WarehouseReceiptDetailDto>>> GetExpiringReceipts(
            [FromQuery] int days = 30, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetExpiringReceiptsQuery(days, page, pageSize), ct));
        [HttpGet("GetInventoryMovementReport")]
        public async Task<ActionResult<IReadOnlyList<InventoryMovementRowDto>>> GetInventoryMovementReport(
            [FromQuery] DateTime? fromUtc = null, [FromQuery] DateTime? toUtc = null,
            CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetInventoryMovementReportQuery(fromUtc, toUtc), ct));
        [HttpGet("GetInventoryBalance")]
        public async Task<ActionResult<IReadOnlyList<InventoryBalanceDto>>> GetInventoryBalance(
            [FromQuery] int? warehouseId = null, [FromQuery] int? itemId = null, CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetInventoryBalanceQuery(warehouseId, itemId), ct));

        [HttpGet("GetWarehouses")]
        public async Task<ActionResult<IReadOnlyList<WarehouseSummaryDto>>> GetWarehouses(
            [FromQuery] bool? isActive = null, CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetWarehousesQuery(isActive), ct));
        [HttpGet("GetWarehouseById/{id}")]
        public async Task<ActionResult<WarehouseSummaryDto>> GetWarehouseById(int id, CancellationToken ct)
        {
            var warehouse = await _mediator.Send(new GetWarehouseByIdQuery(id), ct);
            return warehouse is null ? NotFound() : Ok(warehouse);
        }
        [HttpPut("SetWarehouseActive/{id}")]
        public async Task<IActionResult> SetWarehouseActive(
            int id, [FromBody] SetWarehouseActiveBody body, CancellationToken ct)
            => await _mediator.Send(new SetWarehouseActiveCommand(id, body.IsActive), ct)
                ? NoContent()
                : NotFound();

        public sealed record SetWarehouseActiveBody(bool IsActive);
        [HttpPost("AddWarehouse")]
        public async Task<IActionResult> AddWarehouse(
            [FromBody] CreateWarehouseCommand cmd, CancellationToken ct)
            => (await _mediator.Send(cmd, ct)).ToOk();

        [HttpPost("AddWarehouseReceipt")]
        public async Task<IActionResult> AddWarehouseReceipt(
            [FromBody] CreateWarehouseReceiptCommand cmd, CancellationToken ct)
            => (await _mediator.Send(cmd, ct)).ToOk();

        [HttpPost("AdjustStock")]
        public async Task<IActionResult> AdjustStock(
            [FromBody] AdjustStockCommand cmd, CancellationToken ct)
            => (await _mediator.Send(cmd, ct)).ToNoContent();
    }
}







