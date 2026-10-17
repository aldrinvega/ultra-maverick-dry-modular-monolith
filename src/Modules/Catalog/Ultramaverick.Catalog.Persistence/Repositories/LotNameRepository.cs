using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Repositories;

public sealed class LotNameRepository : ILotNameRepository
{
    private readonly CatalogDbContext _context;

    public LotNameRepository(CatalogDbContext context) => _context = context;

    public Task<LotName?> GetByIdAsync(int id, CancellationToken ct)
        => _context.LotNames.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<LotName?> GetByCodeAsync(string code, CancellationToken ct)
        => _context.LotNames.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<LotName>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive).OrderBy(x => x.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task AddAsync(LotName entity, CancellationToken ct)
        => await _context.LotNames.AddAsync(entity, ct);

    private IQueryable<LotName> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.LotNames.AsNoTracking();

        if (isActive is not null)
            query = query.Where(x => x.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Code.Contains(term) || (x.SectionName != null && x.SectionName.Contains(term)));
        }

        return query;
    }
}