using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Repositories;

public sealed class FormulaRepository : IFormulaRepository
{
    private readonly CatalogDbContext _context;

    public FormulaRepository(CatalogDbContext context) => _context = context;

    public Task<FormulaHeader?> GetByIdAsync(int id, CancellationToken ct)
        => _context.FormulaHeaders
            .Include(f => f.Requirements)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<FormulaHeader?> GetByItemAndVersionAsync(int itemId, int version, CancellationToken ct)
        => _context.FormulaHeaders.AsNoTracking()
            .FirstOrDefaultAsync(f => f.ItemId == itemId && f.Version == version, ct);

    public async Task<IReadOnlyList<FormulaHeader>> ListAsync(
        int? itemId, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(itemId, isActive)
            .Include(f => f.Requirements)
            .OrderBy(f => f.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(int? itemId, bool? isActive, CancellationToken ct)
        => BuildQuery(itemId, isActive).CountAsync(ct);

    public async Task AddAsync(FormulaHeader formula, CancellationToken ct)
        => await _context.FormulaHeaders.AddAsync(formula, ct);

    private IQueryable<FormulaHeader> BuildQuery(int? itemId, bool? isActive)
    {
        var query = _context.FormulaHeaders.AsNoTracking();

        if (itemId is not null)
            query = query.Where(f => f.ItemId == itemId);

        if (isActive is not null)
            query = query.Where(f => f.IsActive == isActive);

        return query;
    }
}