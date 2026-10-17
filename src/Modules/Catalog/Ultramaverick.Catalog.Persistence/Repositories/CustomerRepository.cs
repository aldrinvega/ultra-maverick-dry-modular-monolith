using Microsoft.EntityFrameworkCore;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly CatalogDbContext _context;

    public CustomerRepository(CatalogDbContext context) => _context = context;

    public Task<Customer?> GetByIdAsync(int id, CancellationToken ct)
        => _context.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Customer?> GetByCodeAsync(string code, CancellationToken ct)
        => _context.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.CustomerCode == code, ct);

    public async Task<IReadOnlyList<Customer>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive).OrderBy(x => x.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task AddAsync(Customer entity, CancellationToken ct)
        => await _context.Customers.AddAsync(entity, ct);

    private IQueryable<Customer> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.Customers.AsNoTracking();

        if (isActive is not null)
            query = query.Where(x => x.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.CustomerCode.Contains(term) || x.CustomerName.Contains(term));
        }

        return query;
    }
}