using Microsoft.EntityFrameworkCore;

namespace Ultramaverick.Identity.IntegrationTests;

/// <summary>
/// Proves the transactional outbox: a domain event raised on an entity is written to
/// OutboxMessages in the same SaveChanges as the data change.
/// </summary>
public class OutboxTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OutboxTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Creating_a_user_writes_one_UserChanged_row()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);

        var userId = await TestData.CreateUserAsync(context);

        var rows = await context.OutboxMessages.AsNoTracking().ToListAsync();

        Assert.Single(rows);
        Assert.Equal("UserChanged", rows[0].EventType);
        Assert.Equal("User", rows[0].AggregateType);
        Assert.Equal(userId.ToString(), rows[0].AggregateId);
        Assert.Null(rows[0].PublishedAtUtc);
        Assert.Equal(0, rows[0].Attempts);
        Assert.Contains("Created", rows[0].Payload);
    }

    [Fact]
    public async Task Deactivating_a_user_appends_a_second_row()
    {
        await using var context = _fixture.CreateContext();

        var userId = await TestData.CreateUserAsync(context);
        await TestData.ClearOutboxAsync(context);

        var user = await context.Users.FirstAsync(u => u.Id == userId);
        user.Deactivate(1);
        await context.SaveChangesAsync();

        var rows = await context.OutboxMessages.AsNoTracking().ToListAsync();

        Assert.Single(rows);
        Assert.Equal("UserChanged", rows[0].EventType);
        Assert.Contains("Deactivated", rows[0].Payload);
    }

    [Fact]
    public async Task Deactivating_twice_writes_only_one_row()
    {
        await using var context = _fixture.CreateContext();

        var userId = await TestData.CreateUserAsync(context);
        await TestData.ClearOutboxAsync(context);

        var user = await context.Users.FirstAsync(u => u.Id == userId);
        user.Deactivate(1);
        user.Deactivate(1);
        await context.SaveChangesAsync();

        var rows = await context.OutboxMessages.AsNoTracking().ToListAsync();

        Assert.Single(rows);
    }
}
