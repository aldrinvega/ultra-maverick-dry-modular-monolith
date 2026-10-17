using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Repositories;

public sealed class SupplierRepository : ISupplierRepository
{
    private readonly CatalogDbContext _context;

    public SupplierRepository(CatalogDbContext context) => _context = context;

    public Task<Supplier?> GetByIdAsync(int id, CancellationToken ct)
        => _context.Suppliers.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Supplier?> GetByCodeAsync(string code, CancellationToken ct)
        => _context.Suppliers.AsNoTracking().FirstOrDefaultAsync(x => x.SupplierCode == code, ct);

    public async Task<IReadOnlyList<Supplier>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive).OrderBy(x => x.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task AddAsync(Supplier entity, CancellationToken ct)
        => await _context.Suppliers.AddAsync(entity, ct);

    private IQueryable<Supplier> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.Suppliers.AsNoTracking();

        if (isActive is not null)
            query = query.Where(x => x.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.SupplierCode.Contains(term) || x.SupplierName.Contains(term));
        }

        return query;
    }
}