using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Repositories;

public sealed class FarmRepository : IFarmRepository
{
    private readonly CatalogDbContext _context;

    public FarmRepository(CatalogDbContext context) => _context = context;

    public Task<Farm?> GetByIdAsync(int id, CancellationToken ct)
        => _context.Farms.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Farm?> GetByCodeAsync(string code, CancellationToken ct)
        => _context.Farms.AsNoTracking().FirstOrDefaultAsync(x => x.FarmCode == code, ct);

    public async Task<IReadOnlyList<Farm>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive).OrderBy(x => x.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task AddAsync(Farm entity, CancellationToken ct)
        => await _context.Farms.AddAsync(entity, ct);

    private IQueryable<Farm> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.Farms.AsNoTracking();

        if (isActive is not null)
            query = query.Where(x => x.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.FarmCode.Contains(term) || x.FarmName.Contains(term));
        }

        return query;
    }
}