using Microsoft.EntityFrameworkCore;
using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Application.Queries.PurchaseOrders;

namespace Ultramaverick.Procurement.Persistence.Repositories;

public sealed class PurchaseOrderReader : IPurchaseOrderReader
{
    private readonly ProcurementDbContext _context;

    public PurchaseOrderReader(ProcurementDbContext context) => _context = context;

    public async Task<PurchaseOrderDetailDto?> GetByIdAsync(int purchaseOrderId, CancellationToken ct)
        => await _context.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.Id == purchaseOrderId)
            .Select(x => new PurchaseOrderDetailDto(
                x.Id,
                x.PoNumber,
                x.PrNumber,
                x.PoDate,
                x.PrDate,
                x.SupplierId,
                x.Status,
                x.IsActive,
                x.Lines
                    .OrderBy(l => l.Id)
                    .Select(l => new PurchaseOrderLineDto(
                        l.Id, l.ItemId, l.UomId, l.OrderedQuantity,
                        l.DeliveredQuantity, l.BilledQuantity, l.UnitPrice, l.IsActive))
                    .ToList()))
            .FirstOrDefaultAsync(ct);
}
