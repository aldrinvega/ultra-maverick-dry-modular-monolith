using Microsoft.EntityFrameworkCore;
using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Domain.Entities;

namespace Ultramaverick.Reporting.Persistence.Repositories;

public sealed class InventoryPositionStore : IInventoryPositionStore
{
    private readonly ReportingDbContext _context;

    public InventoryPositionStore(ReportingDbContext context) => _context = context;

    public Task<InventoryPosition?> FindAsync(int warehouseId, int itemId, string lotNo, CancellationToken ct)
        => _context.InventoryPositions
            .FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ItemId == itemId && x.LotNo == lotNo, ct);

    public async Task AddAsync(InventoryPosition position, CancellationToken ct)
        => await _context.InventoryPositions.AddAsync(position, ct);
}
