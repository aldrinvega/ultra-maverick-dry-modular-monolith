using Ultramaverick.Reporting.Domain.Entities;

namespace Ultramaverick.Reporting.Application.Abstractions;

public interface IInventoryPositionStore
{
    Task<InventoryPosition?> FindAsync(int warehouseId, int itemId, string lotNo, CancellationToken ct);
    Task AddAsync(InventoryPosition position, CancellationToken ct);
}
