using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Application.Abstractions;

namespace Ultramaverick.Catalog.Persistence.Repositories;

/// <summary>
/// Generic EF implementation over the simple code/name reference tables. Query predicates
/// address the conventional Id/Code/Name/IsActive properties through EF.Property so one
/// implementation serves every lookup entity.
/// </summary>
public sealed class LookupRepository<T> : ILookupRepository<T> where T : class
{
    private readonly CatalogDbContext _context;

    public LookupRepository(CatalogDbContext context) => _context = context;

    public Task<T?> GetByIdAsync(int id, CancellationToken ct)
        => _context.Set<T>().FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);

    public Task<T?> GetByNameAsync(string name, CancellationToken ct)
        => _context.Set<T>().AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<string>(e, "Name") == name, ct);

    public Task<T?> GetByCodeAsync(string code, CancellationToken ct)
        => _context.Set<T>().AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<string?>(e, "Code") == code, ct);

    public async Task<IReadOnlyList<T>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive)
            .OrderBy(e => EF.Property<int>(e, "Id"))
            .Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct)
        => await _context.Set<T>().AddAsync(entity, ct);

    private IQueryable<T> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.Set<T>().AsNoTracking();

        if (isActive is not null)
            query = query.Where(e => EF.Property<bool>(e, "IsActive") == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e => EF.Property<string>(e, "Name").Contains(term));
        }

        return query;
    }
}