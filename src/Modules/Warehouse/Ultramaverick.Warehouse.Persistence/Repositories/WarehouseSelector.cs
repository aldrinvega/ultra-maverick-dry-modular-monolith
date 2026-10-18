using Microsoft.EntityFrameworkCore;
using Ultramaverick.Warehouse.Application.Abstractions;

namespace Ultramaverick.Warehouse.Persistence.Repositories;

/// <summary>
/// Default-warehouse policy: the lowest-id active warehouse receives cross-module stock.
/// Throws when no active warehouse exists so the failure is explicit (and retryable).
/// </summary>
public sealed class WarehouseSelector : IWarehouseSelector
{
    private readonly WarehouseDbContext _context;

    public WarehouseSelector(WarehouseDbContext context) => _context = context;

    public async Task<int> DefaultForItemAsync(int itemId, CancellationToken ct)
    {
        _ = itemId; // no per-item placement routing yet; the default warehouse is global (G14).

        var warehouseId = await _context.Warehouses
            .Where(w => w.IsActive)
            .OrderBy(w => w.Id)
            .Select(w => (int?)w.Id)
            .FirstOrDefaultAsync(ct);

        return warehouseId
            ?? throw new InvalidOperationException("No active warehouse is configured to receive stock.");
    }
}
