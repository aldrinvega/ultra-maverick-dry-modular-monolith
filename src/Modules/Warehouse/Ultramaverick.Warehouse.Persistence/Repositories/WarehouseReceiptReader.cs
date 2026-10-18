using Microsoft.EntityFrameworkCore;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Queries.WarehouseReceipts;

namespace Ultramaverick.Warehouse.Persistence.Repositories;

public sealed class WarehouseReceiptReader : IWarehouseReceiptReader
{
    private readonly WarehouseDbContext _context;

    public WarehouseReceiptReader(WarehouseDbContext context) => _context = context;

    public async Task<WarehouseReceiptDetailDto?> GetByIdAsync(int receiptId, CancellationToken ct)
        => await _context.WarehouseReceipts
            .AsNoTracking()
            .Where(x => x.Id == receiptId)
            .Select(x => new WarehouseReceiptDetailDto(
                x.Id,
                x.QcReceiptId,
                x.WarehouseId,
                x.ItemId,
                x.UomId,
                x.LotNo,
                x.Quantity,
                x.ManufacturingDate,
                x.ExpiryDate,
                x.ReceivedAtUtc,
                x.ReceivedByUserId,
                x.IsActive))
            .FirstOrDefaultAsync(ct);
}
