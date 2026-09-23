using Microsoft.EntityFrameworkCore;
using Ultramaverick.Identity.Persistence.Repositories;

namespace Ultramaverick.Identity.IntegrationTests;

/// <summary>
/// Proves refresh-token rotation semantics against a real database:
/// single use, expiry, and revocation.
/// </summary>
public class RefreshTokenStoreTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public RefreshTokenStoreTests(DatabaseFixture fixture) => _fixture = fixture;

    private static string Token() => $"t{Guid.NewGuid():N}";

    [Fact]
    public async Task Rotate_consumes_the_presented_token_and_issues_a_replacement()
    {
        await using var context = _fixture.CreateContext();
        var store = new RefreshTokenStore(context);
        var userId = await TestData.CreateUserAsync(context);

        var first = Token();
        var second = Token();
        var third = Token();

        await store.IssueAsync(userId, first, DateTime.UtcNow.AddDays(7), default);

        var rotated = await store.RotateAsync(first, second, DateTime.UtcNow.AddDays(7), default);
        Assert.Equal(userId, rotated);

        // The replacement works.
        var again = await store.RotateAsync(second, third, DateTime.UtcNow.AddDays(7), default);
        Assert.Equal(userId, again);
    }

    [Fact]
    public async Task A_rotated_token_cannot_be_used_again()
    {
        await using var context = _fixture.CreateContext();
        var store = new RefreshTokenStore(context);
        var userId = await TestData.CreateUserAsync(context);

        var first = Token();
        var second = Token();

        await store.IssueAsync(userId, first, DateTime.UtcNow.AddDays(7), default);
        await store.RotateAsync(first, second, DateTime.UtcNow.AddDays(7), default);

        var reused = await store.RotateAsync(first, Token(), DateTime.UtcNow.AddDays(7), default);

        Assert.Null(reused);
    }

    [Fact]
    public async Task An_expired_token_is_rejected()
    {
        await using var context = _fixture.CreateContext();
        var store = new RefreshTokenStore(context);
        var userId = await TestData.CreateUserAsync(context);

        var expired = Token();
        await store.IssueAsync(userId, expired, DateTime.UtcNow.AddMinutes(-1), default);

        var rotated = await store.RotateAsync(expired, Token(), DateTime.UtcNow.AddDays(7), default);

        Assert.Null(rotated);
    }

    [Fact]
    public async Task A_revoked_token_is_rejected()
    {
        await using var context = _fixture.CreateContext();
        var store = new RefreshTokenStore(context);
        var userId = await TestData.CreateUserAsync(context);

        var revoked = Token();
        await store.IssueAsync(userId, revoked, DateTime.UtcNow.AddDays(7), default);
        await store.RevokeAsync(revoked, default);

        var rotated = await store.RotateAsync(revoked, Token(), DateTime.UtcNow.AddDays(7), default);

        Assert.Null(rotated);
    }

    [Fact]
    public async Task The_raw_token_is_never_stored()
    {
        await using var context = _fixture.CreateContext();
        var store = new RefreshTokenStore(context);
        var userId = await TestData.CreateUserAsync(context);

        var raw = Token();
        await store.IssueAsync(userId, raw, DateTime.UtcNow.AddDays(7), default);

        var stored = await context.RefreshTokens.AsNoTracking().SingleAsync(t => t.UserId == userId);
        Assert.NotEqual(raw, stored.TokenHash);
        Assert.Equal(44, stored.TokenHash.Length); // base64 of a SHA-256 hash
    }
}
