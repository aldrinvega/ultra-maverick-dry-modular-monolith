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

    public async Task<IReadOnlyList<int>> GetModuleIdsForRoleAsync(int roleId, CancellationToken ct)
        => await _context.RoleModules
            .Where(rm => rm.RoleId == roleId && rm.IsActive)
            .Select(rm => rm.ModuleId)
            .ToListAsync(ct);

    public async Task SetModulesAsync(
        int roleId, IReadOnlyCollection<int> moduleIds, int modifiedByUserId, CancellationToken ct)
    {
        var desired = moduleIds.Distinct().ToHashSet();

        var existing = await _context.RoleModules
            .Where(rm => rm.RoleId == roleId)
            .ToListAsync(ct);

        foreach (var moduleId in desired)
        {
            var link = existing.FirstOrDefault(rm => rm.ModuleId == moduleId);

            if (link is null)
                _context.RoleModules.Add(RoleModule.Create(roleId, moduleId));
            else if (!link.IsActive)
                link.Activate(modifiedByUserId);
        }

        foreach (var link in existing.Where(rm => rm.IsActive && !desired.Contains(rm.ModuleId)))
            link.Deactivate(modifiedByUserId);
    }

    public async Task DeactivateModulesAsync(
        int roleId, IReadOnlyCollection<int> moduleIds, int modifiedByUserId, CancellationToken ct)
    {
        var target = moduleIds.Distinct().ToHashSet();

        var links = await _context.RoleModules
            .Where(rm => rm.RoleId == roleId && target.Contains(rm.ModuleId) && rm.IsActive)
            .ToListAsync(ct);

        foreach (var link in links)
            link.Deactivate(modifiedByUserId);
    }

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
