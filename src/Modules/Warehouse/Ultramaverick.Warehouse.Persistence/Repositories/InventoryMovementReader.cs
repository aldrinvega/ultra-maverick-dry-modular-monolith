using Microsoft.EntityFrameworkCore;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Queries.Inventory;

namespace Ultramaverick.Warehouse.Persistence.Repositories;

public sealed class InventoryMovementReader : IInventoryMovementReader
{
    private readonly WarehouseDbContext _context;

    public InventoryMovementReader(WarehouseDbContext context) => _context = context;

    public async Task<IReadOnlyList<InventoryMovementRowDto>> GetAsync(
        DateTime? fromUtc, DateTime? toUtc, CancellationToken ct)
    {
        var query = _context.InventoryMovements.AsNoTracking();

        if (fromUtc is not null)
            query = query.Where(x => x.OccurredAtUtc >= fromUtc.Value);

        if (toUtc is not null)
            query = query.Where(x => x.OccurredAtUtc <= toUtc.Value);

        return await query
            .GroupBy(x => new { x.ItemId, x.WarehouseId })
            .OrderBy(g => g.Key.ItemId)
            .ThenBy(g => g.Key.WarehouseId)
            .Select(g => new InventoryMovementRowDto(
                g.Key.ItemId,
                g.Key.WarehouseId,
                g.Sum(x => x.Delta > 0 ? x.Delta : 0),
                -g.Sum(x => x.Delta < 0 ? x.Delta : 0),
                g.Sum(x => x.Delta)))
            .ToListAsync(ct);
    }
}
