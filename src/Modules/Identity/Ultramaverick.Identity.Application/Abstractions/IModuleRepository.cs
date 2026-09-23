using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface IModuleRepository
    {
        Task<IReadOnlyList<string>> GetModuleNamesForRoleAsync(int roleId, CancellationToken ct);
        Task<Module?> GetByIdAsync(int moduleId, CancellationToken ct);
        Task<IReadOnlyList<Module>> GetByRoleIdAsync(int roleId, CancellationToken ct);
        Task<IReadOnlyList<Module>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
        Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
    }
}
