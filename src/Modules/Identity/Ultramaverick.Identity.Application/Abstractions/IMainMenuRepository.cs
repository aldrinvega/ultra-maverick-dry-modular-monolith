using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Application.Abstractions
{
    public interface IMainMenuRepository
    {
        Task<MainMenu?> GetByIdAsync(int menuId, CancellationToken ct);
        Task<IReadOnlyList<MainMenu>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct);
        Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct);
        Task AddAsync(MainMenu menu, CancellationToken ct);
    }
}
