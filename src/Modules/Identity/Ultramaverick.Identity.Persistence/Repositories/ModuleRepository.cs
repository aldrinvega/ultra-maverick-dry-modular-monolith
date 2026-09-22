using Microsoft.EntityFrameworkCore;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Persistence.Repositories;

public sealed class ModuleRepository : IModuleRepository
{
    private readonly IdentityDbContext _context;
    public ModuleRepository(IdentityDbContext context) => _context = context;

    public async Task<IReadOnlyList<string>> GetModuleNamesForRoleAsync(int roleId, CancellationToken ct)
    {
        var moduleIds = _context.RoleModules
            .Where(rm => rm.RoleId == roleId && rm.IsActive)
            .Select(rm => rm.ModuleId);

        return await _context.Modules
            .Where(m => m.IsActive && moduleIds.Contains(m.Id))
            .Select(m => m.Name)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Module>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive).OrderBy(m => m.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    private IQueryable<Module> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.Modules.AsNoTracking();

        if (isActive is not null)
            query = query.Where(m => m.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m => m.Name.Contains(term)
                                  || (m.SubMenuName != null && m.SubMenuName.Contains(term)));
        }

        return query;
    }
}
