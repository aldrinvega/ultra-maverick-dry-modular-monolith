using Ultramaverick.Warehouse.Application.Queries.Inventory;

namespace Ultramaverick.Warehouse.Application.Abstractions;

public interface IInventoryMovementReader
{
    Task<IReadOnlyList<InventoryMovementRowDto>> GetAsync(
        DateTime? fromUtc, DateTime? toUtc, CancellationToken ct);
}
