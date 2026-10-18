using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Application.Abstractions;

public interface IWarehouseReceiptRepository
{
    Task<bool> ExistsForQcReceiptAsync(int qcReceiptId, CancellationToken ct);
    Task<WarehouseReceipt?> GetByIdAsync(int warehouseReceiptId, CancellationToken ct);
    Task<IReadOnlyList<WarehouseReceipt>> ListAsync(int skip, int take, CancellationToken ct);
    Task<int> CountAsync(CancellationToken ct);
    Task AddAsync(WarehouseReceipt receipt, CancellationToken ct);
}
