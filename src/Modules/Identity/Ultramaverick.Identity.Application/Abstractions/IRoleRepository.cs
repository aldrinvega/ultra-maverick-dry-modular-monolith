using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(int roleId, CancellationToken ct);
        Task<Role?> GetByNameAsync(string roleName, CancellationToken ct);
        Task<IReadOnlyList<Role>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
        Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
        Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct);
        Task AddAsync(Role role, CancellationToken ct);

        /// <summary>Module ids currently granted to a role (active grants only).</summary>
        Task<IReadOnlyList<int>> GetModuleIdsForRoleAsync(int roleId, CancellationToken ct);

        /// <summary>
        /// Replaces a role's active module set: adds missing grants, reactivates inactive
        /// ones, and deactivates grants that are no longer requested. Does not save.
        /// </summary>
        Task SetModulesAsync(int roleId, IReadOnlyCollection<int> moduleIds, int modifiedByUserId, CancellationToken ct);

        /// <summary>Deactivates the listed grants for a role, leaving other grants untouched. Does not save.</summary>
        Task DeactivateModulesAsync(int roleId, IReadOnlyCollection<int> moduleIds, int modifiedByUserId, CancellationToken ct);
    }
}
