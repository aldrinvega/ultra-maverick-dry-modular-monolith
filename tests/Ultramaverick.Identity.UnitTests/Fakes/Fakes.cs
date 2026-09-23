using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.UnitTests.Fakes;

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public void Seed(User user) => _users.Add(user);

    public Task<User?> GetByIdAsync(int userId, CancellationToken ct)
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == userId));

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken ct)
        => Task.FromResult(_users.FirstOrDefault(u => u.UserName == userName));

    public Task<IReadOnlyList<User>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => throw new NotSupportedException();

    public Task AddAsync(User user, CancellationToken ct)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }
}

internal sealed class FakeRoleRepository : IRoleRepository
{
    private readonly List<Role> _roles = new();

    public void Seed(Role role) => _roles.Add(role);

    public Task<Role?> GetByIdAsync(int roleId, CancellationToken ct)
        // Test double: entities created via Role.Create have Id 0 until EF assigns one,
        // so fall back to the single seeded role when no id matches.
        => Task.FromResult(_roles.FirstOrDefault(r => r.Id == roleId) ?? _roles.FirstOrDefault());

    public Task<Role?> GetByNameAsync(string roleName, CancellationToken ct)
        => Task.FromResult(_roles.FirstOrDefault(r => r.Name == roleName));

    public Task<IReadOnlyList<Role>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Role>>(_roles);

    public Task AddAsync(Role role, CancellationToken ct)
    {
        _roles.Add(role);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<int>> GetModuleIdsForRoleAsync(int roleId, CancellationToken ct)
        => throw new NotSupportedException();

    public Task SetModulesAsync(int roleId, IReadOnlyCollection<int> moduleIds, int modifiedByUserId, CancellationToken ct)
        => throw new NotSupportedException();

    public Task DeactivateModulesAsync(int roleId, IReadOnlyCollection<int> moduleIds, int modifiedByUserId, CancellationToken ct)
        => throw new NotSupportedException();
}

internal sealed class FakeModuleRepository : IModuleRepository
{
    private readonly List<string> _names = new();

    public void Seed(params string[] names) => _names.AddRange(names);

    public Task<IReadOnlyList<string>> GetModuleNamesForRoleAsync(int roleId, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<string>>(_names);

    public Task<Module?> GetByIdAsync(int moduleId, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<Module>> GetByRoleIdAsync(int roleId, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<int>> GetExistingIdsAsync(IReadOnlyCollection<int> moduleIds, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<Module>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => throw new NotSupportedException();

    public Task AddAsync(Module module, CancellationToken ct)
        => throw new NotSupportedException();
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public bool VerifyResult { get; set; } = true;

    public string Hash(string plaintext) => $"hashed::{plaintext}";

    public bool Verify(string plaintext, string encodedHash) => VerifyResult;
}

internal sealed class FakeTokenService : ITokenService
{
    public int RefreshTokenDaysValue { get; set; } = 7;

    public AccessToken CreateAccessToken(User user, IReadOnlyCollection<string> moduleNames)
        => new("access-token", DateTime.UtcNow.AddMinutes(60));

    public string CreateRefreshToken() => "new-refresh-token";

    public int RefreshTokenDays => RefreshTokenDaysValue;
}

internal sealed class FakeRefreshTokenStore : IRefreshTokenStore
{
    public int IssuedCount { get; private set; }

    public Task<string> IssueAsync(int userId, string token, DateTime expiresAtUtc, CancellationToken ct)
    {
        IssuedCount++;
        return Task.FromResult(token);
    }

    public Task<int?> ConsumeAsync(string token, CancellationToken ct) => Task.FromResult<int?>(null);
    public Task<int?> RotateAsync(string presentedToken, string newToken, DateTime newExpiresAtUtc, CancellationToken ct)
        => Task.FromResult<int?>(null);
    public Task RevokeAsync(string token, CancellationToken ct) => Task.CompletedTask;
    public Task RevokeAllForUserAsync(int userId, CancellationToken ct) => Task.CompletedTask;
}

internal sealed class FakeCurrentUser : ICurrentUser
{
    public int? UserId { get; set; } = 1;
    public string? UserName { get; set; } = "tester";
    public bool IsAuthenticated { get; set; } = true;
}
