using Microsoft.EntityFrameworkCore;
using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Domain.Entities;

namespace Ultramaverick.Procurement.Persistence.Repositories;

public sealed class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly ProcurementDbContext _context;

    public PurchaseOrderRepository(ProcurementDbContext context) => _context = context;

    public Task<PurchaseOrder?> GetByNumberAsync(string poNumber, CancellationToken ct)
        => _context.PurchaseOrders.AsNoTracking().FirstOrDefaultAsync(x => x.PoNumber == poNumber, ct);

    public async Task<IReadOnlyList<PurchaseOrder>> ListAsync(
        string? search, int skip, int take, CancellationToken ct)
        => await BuildQuery(search).OrderBy(x => x.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, CancellationToken ct)
        => BuildQuery(search).CountAsync(ct);

    public async Task AddAsync(PurchaseOrder entity, CancellationToken ct)
        => await _context.PurchaseOrders.AddAsync(entity, ct);

    private IQueryable<PurchaseOrder> BuildQuery(string? search)
    {
        var query = _context.PurchaseOrders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            if (term.Length > 100) term = term[..100];
            query = query.Where(x => x.PoNumber.Contains(term)
                || (x.PrNumber != null && x.PrNumber.Contains(term)));
        }

        return query;
    }
}
