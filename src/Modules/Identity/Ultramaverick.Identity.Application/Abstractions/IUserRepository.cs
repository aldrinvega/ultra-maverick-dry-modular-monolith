using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int userId, CancellationToken ct);
        Task<User?> GetByUserNameAsync(string userName, CancellationToken ct);
        Task<IReadOnlyList<User>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
        Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
        Task AddAsync(User user, CancellationToken ct);
    }
}
