using Microsoft.EntityFrameworkCore;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Models;
using Ultramaverick.Warehouse.Application.Queries.WarehouseReceipts;

namespace Ultramaverick.Warehouse.Persistence.Repositories;

public sealed class WarehouseReceiptReader : IWarehouseReceiptReader
{
    private readonly WarehouseDbContext _context;

    public WarehouseReceiptReader(WarehouseDbContext context) => _context = context;

    public async Task<WarehouseReceiptDetailDto?> GetByIdAsync(int receiptId, CancellationToken ct)
        => await Projected(_context.WarehouseReceipts.AsNoTracking().Where(x => x.Id == receiptId))
            .FirstOrDefaultAsync(ct);

    public async Task<PagedResult<WarehouseReceiptDetailDto>> GetExpiringPagedAsync(
        int days, int page, int pageSize, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(days);

        var source = _context.WarehouseReceipts
            .AsNoTracking()
            .Where(x => x.ExpiryDate != null && x.ExpiryDate <= cutoff);

        var totalCount = await source.CountAsync(ct);

        var items = await Projected(source
                .OrderBy(x => x.ExpiryDate)
                .ThenBy(x => x.Id))
            .Skip(Offset(page, pageSize))
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WarehouseReceiptDetailDto>(items, page, pageSize, totalCount);
    }

    private static int Offset(int page, int pageSize)
    {
        var offset = (long)(page - 1) * pageSize;
        return offset > int.MaxValue ? int.MaxValue : (int)Math.Max(0, offset);
    }

    private static IQueryable<WarehouseReceiptDetailDto> Projected(
        IQueryable<Domain.Entities.WarehouseReceipt> source)
        => source.Select(x => new WarehouseReceiptDetailDto(
            x.Id, x.QcReceiptId, x.WarehouseId, x.ItemId, x.UomId, x.LotNo, x.Quantity,
            x.ManufacturingDate, x.ExpiryDate, x.ReceivedAtUtc, x.ReceivedByUserId, x.IsActive));
}
