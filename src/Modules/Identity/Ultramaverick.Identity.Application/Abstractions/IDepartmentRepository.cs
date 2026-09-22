using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface IDepartmentRepository
    {
        Task<Department?> GetByIdAsync(int departmentId, CancellationToken ct);
        Task<IReadOnlyList<Department>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
        Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
        Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken ct);
    }
}
