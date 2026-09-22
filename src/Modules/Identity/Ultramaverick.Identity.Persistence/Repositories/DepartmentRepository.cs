using Microsoft.EntityFrameworkCore;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Persistence.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly IdentityDbContext _context;
    public DepartmentRepository(IdentityDbContext context) => _context = context;

    public Task<Department?> GetByIdAsync(int departmentId, CancellationToken ct)
        => _context.Departments.FirstOrDefaultAsync(d => d.Id == departmentId, ct);

    public async Task<IReadOnlyList<Department>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive).OrderBy(d => d.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken ct)
        => await _context.Departments.AsNoTracking().OrderBy(d => d.Id).ToListAsync(ct);

    private IQueryable<Department> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.Departments.AsNoTracking();

        if (isActive is not null)
            query = query.Where(d => d.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d => d.Name.Contains(term));
        }

        return query;
    }
}
