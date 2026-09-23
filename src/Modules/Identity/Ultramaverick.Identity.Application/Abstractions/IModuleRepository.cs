using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface IModuleRepository
    {
        Task<IReadOnlyList<string>> GetModuleNamesForRoleAsync(int roleId, CancellationToken ct);
        Task<Module?> GetByIdAsync(int moduleId, CancellationToken ct);
        Task<IReadOnlyList<Module>> GetByRoleIdAsync(int roleId, CancellationToken ct);

        /// <summary>Returns the subset of the supplied ids that exist as modules.</summary>
        Task<IReadOnlyList<int>> GetExistingIdsAsync(IReadOnlyCollection<int> moduleIds, CancellationToken ct);

        Task<IReadOnlyList<Module>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
        Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
        Task AddAsync(Module module, CancellationToken ct);
    }
}
