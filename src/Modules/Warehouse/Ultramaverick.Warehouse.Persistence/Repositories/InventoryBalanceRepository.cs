using Microsoft.EntityFrameworkCore;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Persistence.Repositories;

public sealed class InventoryBalanceRepository : IInventoryBalanceRepository
{
    private readonly WarehouseDbContext _context;

    public InventoryBalanceRepository(WarehouseDbContext context) => _context = context;

    // Plain tracked read (no UPDLOCK/HOLDLOCK): the row version handles lost-update detection.
    public Task<InventoryBalance?> GetForUpdateAsync(
        int warehouseId, int itemId, int uomId, string? lotNo, CancellationToken ct)
        => _context.InventoryBalances.FirstOrDefaultAsync(
            b => b.WarehouseId == warehouseId
              && b.ItemId == itemId
              && b.UomId == uomId
              && (lotNo == null ? b.LotNo == null : b.LotNo == lotNo),
            ct);

    public async Task<IReadOnlyList<InventoryBalance>> ListAsync(
        int? warehouseId, int? itemId, CancellationToken ct)
        => await _context.InventoryBalances
            .AsNoTracking()
            .Where(b => (warehouseId == null || b.WarehouseId == warehouseId)
                     && (itemId == null || b.ItemId == itemId))
            .OrderBy(b => b.Id)
            .ToListAsync(ct);

    public async Task AddAsync(InventoryBalance balance, CancellationToken ct)
        => await _context.InventoryBalances.AddAsync(balance, ct);
}
