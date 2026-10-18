using Ultramaverick.Warehouse.Application.Queries.WarehouseReceipts;

namespace Ultramaverick.Warehouse.Application.Abstractions;

public interface IWarehouseReceiptReader
{
    Task<WarehouseReceiptDetailDto?> GetByIdAsync(int receiptId, CancellationToken ct);
}
