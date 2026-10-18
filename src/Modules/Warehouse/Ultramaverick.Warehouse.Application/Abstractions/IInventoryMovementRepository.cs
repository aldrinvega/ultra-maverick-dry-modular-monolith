using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Application.Abstractions;

public interface IInventoryMovementRepository
{
    Task AddAsync(InventoryMovement movement, CancellationToken ct);
    Task<IReadOnlyList<InventoryMovement>> ListForAsync(int warehouseId, int itemId, CancellationToken ct);
}
