using Microsoft.EntityFrameworkCore;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _context;
    public RoleRepository(IdentityDbContext context) => _context = context;

    public Task<Role?> GetByIdAsync(int roleId, CancellationToken ct)
        => _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, ct);

    public Task<Role?> GetByNameAsync(string roleName, CancellationToken ct)
        => _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName, ct);

    public async Task<IReadOnlyList<Role>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive).OrderBy(r => r.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct)
        => await _context.Roles.AsNoTracking().OrderBy(r => r.Id).ToListAsync(ct);

    public async Task AddAsync(Role role, CancellationToken ct)
        => await _context.Roles.AddAsync(role, ct);

    private IQueryable<Role> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.Roles.AsNoTracking();

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
