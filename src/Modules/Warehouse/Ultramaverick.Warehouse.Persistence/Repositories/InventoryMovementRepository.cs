using Microsoft.EntityFrameworkCore;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Persistence.Repositories;

public sealed class InventoryMovementRepository : IInventoryMovementRepository
{
    private readonly WarehouseDbContext _context;

    public InventoryMovementRepository(WarehouseDbContext context) => _context = context;

    public async Task AddAsync(InventoryMovement movement, CancellationToken ct)
        => await _context.InventoryMovements.AddAsync(movement, ct);

    public async Task<IReadOnlyList<InventoryMovement>> ListForAsync(
        int warehouseId, int itemId, CancellationToken ct)
        => await _context.InventoryMovements
            .AsNoTracking()
            .Where(m => m.WarehouseId == warehouseId && m.ItemId == itemId)
            .OrderBy(m => m.Id)
            .ToListAsync(ct);
}
