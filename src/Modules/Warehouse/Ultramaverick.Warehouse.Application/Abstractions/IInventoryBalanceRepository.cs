using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Application.Abstractions;

public interface IInventoryBalanceRepository
{
    /// <summary>Tracked read of the balance for a (warehouse, item, uom, lot) key; null when none.</summary>
    Task<InventoryBalance?> GetForUpdateAsync(int warehouseId, int itemId, int uomId, string? lotNo, CancellationToken ct);

    Task<IReadOnlyList<InventoryBalance>> ListAsync(int? warehouseId, int? itemId, CancellationToken ct);

    Task AddAsync(InventoryBalance balance, CancellationToken ct);
}
