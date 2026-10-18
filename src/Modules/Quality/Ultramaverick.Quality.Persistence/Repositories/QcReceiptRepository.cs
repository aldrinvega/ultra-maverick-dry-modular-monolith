using Microsoft.EntityFrameworkCore;
using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.Persistence.Repositories;

public sealed class QcReceiptRepository : IQcReceiptRepository
{
    private readonly QualityDbContext _context;

    public QcReceiptRepository(QualityDbContext context) => _context = context;

    public Task<bool> ExistsForPurchaseOrderAsync(int purchaseOrderId, CancellationToken ct)
        => _context.QcReceipts.AsNoTracking().AnyAsync(x => x.PurchaseOrderId == purchaseOrderId, ct);

    // Tracked (no AsNoTracking) so Reject can mutate the loaded aggregate; Rejects is
    // eager-loaded so a new reject is attached to the tracked collection and inserted.
    public Task<QcReceipt?> GetByIdAsync(int qcReceiptId, CancellationToken ct)
        => _context.QcReceipts
            .Include(x => x.Rejects)
            .FirstOrDefaultAsync(x => x.Id == qcReceiptId, ct);

    public async Task<IReadOnlyList<QcReceipt>> ListAsync(
        string? search, int skip, int take, CancellationToken ct)
        => await BuildQuery(search).OrderBy(x => x.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, CancellationToken ct)
        => BuildQuery(search).CountAsync(ct);

    public async Task AddAsync(QcReceipt entity, CancellationToken ct)
        => await _context.QcReceipts.AddAsync(entity, ct);

    private IQueryable<QcReceipt> BuildQuery(string? search)
    {
        var query = _context.QcReceipts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            if (term.Length > 100) term = term[..100];
            query = query.Where(x => (x.BatchNo != null && x.BatchNo.Contains(term))
                || x.Status.Contains(term));
        }

        return query;
    }
}
