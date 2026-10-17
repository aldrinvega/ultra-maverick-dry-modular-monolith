using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Repositories;

public sealed class ReasonRepository : IReasonRepository
{
    private readonly CatalogDbContext _context;

    public ReasonRepository(CatalogDbContext context) => _context = context;

    public Task<Reason?> GetByIdAsync(int id, CancellationToken ct)
        => _context.Reasons.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Reason?> GetByNameAsync(string name, CancellationToken ct)
        => _context.Reasons.AsNoTracking().FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<IReadOnlyList<Reason>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive).OrderBy(r => r.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task AddAsync(Reason reason, CancellationToken ct)
        => await _context.Reasons.AddAsync(reason, ct);

    private IQueryable<Reason> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.Reasons.AsNoTracking();

        if (isActive is not null)
            query = query.Where(r => r.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r => r.Name.Contains(term));
        }

        return query;
    }
}