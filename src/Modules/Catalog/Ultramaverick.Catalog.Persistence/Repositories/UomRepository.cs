using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Repositories;

public sealed class UomRepository : IUomRepository
{
    private readonly CatalogDbContext _context;

    public UomRepository(CatalogDbContext context) => _context = context;

    public Task<Uom?> GetByIdAsync(int id, CancellationToken ct)
        => _context.Uoms.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<Uom?> GetByCodeAsync(string code, CancellationToken ct)
        => _context.Uoms.AsNoTracking().FirstOrDefaultAsync(u => u.Code == code, ct);

    public async Task<IReadOnlyList<Uom>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive).OrderBy(u => u.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task AddAsync(Uom uom, CancellationToken ct)
        => await _context.Uoms.AddAsync(uom, ct);

    private IQueryable<Uom> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.Uoms.AsNoTracking();

        if (isActive is not null)
            query = query.Where(u => u.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => u.Code.Contains(term) || u.Description.Contains(term));
        }

        return query;
    }
}