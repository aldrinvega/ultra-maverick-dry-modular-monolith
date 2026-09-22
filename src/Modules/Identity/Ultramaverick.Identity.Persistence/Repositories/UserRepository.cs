using Microsoft.EntityFrameworkCore;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;
    public UserRepository(IdentityDbContext context) => _context = context;

    public Task<User?> GetByIdAsync(int userId, CancellationToken ct)
        => _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken ct)
        => _context.Users.FirstOrDefaultAsync(u => u.UserName == userName, ct);

    public async Task<IReadOnlyList<User>> ListAsync(
        string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => await BuildQuery(search, isActive).OrderBy(u => u.Id).Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => BuildQuery(search, isActive).CountAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct)
        => await _context.Users.AddAsync(user, ct);

    private IQueryable<User> BuildQuery(string? search, bool? isActive)
    {
        var query = _context.Users.AsNoTracking();

        if (isActive is not null)
            query = query.Where(u => u.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => u.FullName.Contains(term) || u.UserName.Contains(term));
        }

        return query;
    }
}
