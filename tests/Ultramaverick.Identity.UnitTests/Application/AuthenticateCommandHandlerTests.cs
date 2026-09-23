using Ultramaverick.Identity.Application.Commands.Authentication;
using Ultramaverick.Identity.Domain.Entities;
using Ultramaverick.Identity.Domain.ValueObjects;
using Ultramaverick.Identity.UnitTests.Fakes;

namespace Ultramaverick.Identity.UnitTests.Application;

public class AuthenticateCommandHandlerTests
{
    private const string InvalidCredentials = "Username or Password is incorrect!";

    private readonly FakeUserRepository _users = new();
    private readonly FakeRoleRepository _roles = new();
    private readonly FakeModuleRepository _modules = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeTokenService _tokens = new();
    private readonly FakeRefreshTokenStore _refreshStore = new();

    private AuthenticateCommandHandler CreateHandler()
        => new(_users, _roles, _modules, _hasher, _tokens, _refreshStore);

    private User SeedUser(bool isActive = true)
    {
        var user = User.Create("Jane Doe", "jdoe",
            PasswordHash.FromEncoded("PBKDF2;SHA256;210000;AAAA;BBBB"), 1, 1, null);

        if (!isActive) user.Deactivate(1);

        _users.Seed(user);
        _roles.Seed(Role.Create("Administrator", 1));
        _modules.Seed("Identity", "Catalog");
        return user;
    }

    [Fact]
    public async Task Valid_credentials_return_a_token_and_issue_a_refresh_token()
    {
        SeedUser();

        var result = await CreateHandler().Handle(new AuthenticateCommand("jdoe", "pw"), default);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal("Administrator", result.Value!.RoleName);
        Assert.Equal(1, result.Value.RoleId);
        Assert.Equal("access-token", result.Value.AccessToken);
        Assert.Equal("new-refresh-token", result.Value.RefreshToken);
        Assert.Equal(1, _refreshStore.IssuedCount);
    }

    [Fact]
    public async Task Unknown_user_fails_with_the_generic_message()
    {
        _roles.Seed(Role.Create("Administrator", 1));

        var result = await CreateHandler().Handle(new AuthenticateCommand("nobody", "pw"), default);

        Assert.False(result.Succeeded);
        Assert.Equal(InvalidCredentials, result.Error);
        Assert.Equal(0, _refreshStore.IssuedCount);
    }

    [Fact]
    public async Task Inactive_user_fails_with_the_generic_message()
    {
        SeedUser(isActive: false);

        var result = await CreateHandler().Handle(new AuthenticateCommand("jdoe", "pw"), default);

        Assert.False(result.Succeeded);
        Assert.Equal(InvalidCredentials, result.Error);
        Assert.Equal(0, _refreshStore.IssuedCount);
    }

    [Fact]
    public async Task Wrong_password_fails_with_the_generic_message()
    {
        SeedUser();
        _hasher.VerifyResult = false;

        var result = await CreateHandler().Handle(new AuthenticateCommand("jdoe", "wrong"), default);

        Assert.False(result.Succeeded);
        Assert.Equal(InvalidCredentials, result.Error);
        Assert.Equal(0, _refreshStore.IssuedCount);
    }

    [Fact]
    public async Task User_name_is_trimmed_before_lookup()
    {
        SeedUser();

        var result = await CreateHandler().Handle(new AuthenticateCommand("  jdoe  ", "pw"), default);

        Assert.True(result.Succeeded);
    }
}
