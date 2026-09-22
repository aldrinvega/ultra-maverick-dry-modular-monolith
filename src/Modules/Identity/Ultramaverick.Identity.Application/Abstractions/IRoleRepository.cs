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
    }
}
