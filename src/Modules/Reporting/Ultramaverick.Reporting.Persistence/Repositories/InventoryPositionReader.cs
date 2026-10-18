using Microsoft.EntityFrameworkCore;
using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Application.Queries.InventoryPositions;

namespace Ultramaverick.Reporting.Persistence.Repositories;

public sealed class InventoryPositionReader : IInventoryPositionReader
{
    private readonly ReportingDbContext _context;

    public InventoryPositionReader(ReportingDbContext context) => _context = context;

    public async Task<IReadOnlyList<InventoryPositionDto>> GetAsync(int? warehouseId, CancellationToken ct)
    {
        IQueryable<Domain.Entities.InventoryPosition> query = _context.InventoryPositions.AsNoTracking();

        if (warehouseId is not null)
            query = query.Where(x => x.WarehouseId == warehouseId.Value);

        return await query
            .OrderBy(x => x.ItemId)
            .ThenBy(x => x.LotNo)
            .Select(x => new InventoryPositionDto(
                x.WarehouseId, x.ItemId, x.UomId, x.LotNo, x.Quantity, x.UpdatedAtUtc))
            .ToListAsync(ct);
    }
}
