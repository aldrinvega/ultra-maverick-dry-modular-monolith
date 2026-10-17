using Microsoft.EntityFrameworkCore;

namespace Ultramaverick.Catalog.IntegrationTests;

/// <summary>
/// Proves the transactional outbox: a domain event raised on a Catalog entity is written
/// to the shared Infrastructure.OutboxMessages table in the same SaveChanges as the data
/// change, so the dispatcher can pick it up.
/// </summary>
public class OutboxTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OutboxTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Creating_an_item_writes_one_ItemChanged_row()
    {
        await using var context = _fixture.CreateContext();

        var itemId = await TestData.CreateItemAsync(context);

        var rows = await context.OutboxMessages.AsNoTracking().ToListAsync();

        Assert.Single(rows);
        Assert.Equal("ItemChanged", rows[0].EventType);
        Assert.Equal("Item", rows[0].AggregateType);
        Assert.Equal(itemId.ToString(), rows[0].AggregateId);
        Assert.Null(rows[0].PublishedAtUtc);
        Assert.Equal(0, rows[0].Attempts);
        Assert.Contains("Created", rows[0].Payload);
    }

    [Fact]
    public async Task Deactivating_an_item_appends_a_second_row()
    {
        await using var context = _fixture.CreateContext();

        var itemId = await TestData.CreateItemAsync(context);
        await TestData.ClearOutboxAsync(context);

        var item = await context.Items.FirstAsync(i => i.Id == itemId);
        item.SetActive(false, 1);
        await context.SaveChangesAsync();

        var rows = await context.OutboxMessages.AsNoTracking().ToListAsync();

        Assert.Single(rows);
        Assert.Equal("ItemChanged", rows[0].EventType);
        Assert.Contains("Deactivated", rows[0].Payload);
    }

    [Fact]
    public async Task Deactivating_an_item_twice_writes_only_one_row()
    {
        await using var context = _fixture.CreateContext();

        var itemId = await TestData.CreateItemAsync(context);
        await TestData.ClearOutboxAsync(context);

        var item = await context.Items.FirstAsync(i => i.Id == itemId);
        item.SetActive(false, 1);
        item.SetActive(false, 1);
        await context.SaveChangesAsync();

        var rows = await context.OutboxMessages.AsNoTracking().ToListAsync();

        Assert.Single(rows);
    }
}