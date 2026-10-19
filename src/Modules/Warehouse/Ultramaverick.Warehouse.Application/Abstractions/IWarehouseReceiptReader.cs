using Ultramaverick.Warehouse.Application.Models;
using Ultramaverick.Warehouse.Application.Queries.WarehouseReceipts;

namespace Ultramaverick.Warehouse.Application.Abstractions;

public interface IWarehouseReceiptReader
{
    Task<WarehouseReceiptDetailDto?> GetByIdAsync(int receiptId, CancellationToken ct);

    Task<PagedResult<WarehouseReceiptDetailDto>> GetExpiringPagedAsync(int days, int page, int pageSize, CancellationToken ct);
}


