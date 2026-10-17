using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Repositories;

public sealed class ItemRepository : IItemRepository
{
    private readonly CatalogDbContext _context;

    public ItemRepository(CatalogDbContext context) => _context = context;

    public Task<Item?> GetByIdAsync(int id, CancellationToken ct)
        => _context.Items.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Item?> GetByCodeAsync(string code, CancellationToken ct)
        => _context.Items.AsNoTracking().FirstOrDefaultAsync(x => x.ItemCode == code, ct);

    public async Task<IReadOnlyList<Item>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive).OrderBy(x => x.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task AddAsync(Item entity, CancellationToken ct)
        => await _context.Items.AddAsync(entity, ct);

    private IQueryable<Item> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.Items.AsNoTracking();

        if (isActive is not null)
            query = query.Where(x => x.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.ItemCode.Contains(term) || x.ItemDescription.Contains(term));
        }

        return query;
    }
}