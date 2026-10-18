using WarehouseEntity = Ultramaverick.Warehouse.Domain.Entities.Warehouse;

namespace Ultramaverick.Warehouse.Application.Abstractions;

public interface IWarehouseRepository
{
    Task<WarehouseEntity?> GetByIdAsync(int warehouseId, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<WarehouseEntity>> ListAsync(bool? isActive, CancellationToken ct);
    Task AddAsync(WarehouseEntity warehouse, CancellationToken ct);
}
