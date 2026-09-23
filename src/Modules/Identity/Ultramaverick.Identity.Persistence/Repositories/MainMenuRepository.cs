using Microsoft.EntityFrameworkCore;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Persistence.Repositories;

public sealed class MainMenuRepository : IMainMenuRepository
{
    private readonly IdentityDbContext _context;
    public MainMenuRepository(IdentityDbContext context) => _context = context;

    public Task<MainMenu?> GetByIdAsync(int menuId, CancellationToken ct)
        => _context.MainMenus.FirstOrDefaultAsync(m => m.Id == menuId, ct);

    public async Task<IReadOnlyList<MainMenu>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Id)
            .Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task AddAsync(MainMenu menu, CancellationToken ct)
        => await _context.MainMenus.AddAsync(menu, ct);

    private IQueryable<MainMenu> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.MainMenus.AsNoTracking();

        if (isActive is not null)
            query = query.Where(m => m.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m => m.Name.Contains(term) || m.Path.Contains(term));
        }

        return query;
    }
}
