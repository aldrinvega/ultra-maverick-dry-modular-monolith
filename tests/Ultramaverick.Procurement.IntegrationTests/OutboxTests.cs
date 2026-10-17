using Microsoft.EntityFrameworkCore;

namespace Ultramaverick.Procurement.IntegrationTests;

/// <summary>
/// Proves the transactional outbox: a domain event raised on a Procurement entity is written
/// to the module's Procurement.OutboxMessages table in the same SaveChanges as the data
/// change, so the dispatcher can pick it up.
/// </summary>
public class OutboxTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OutboxTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Creating_a_purchase_order_writes_one_PurchaseOrderChanged_row()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);

        var poId = await TestData.CreatePurchaseOrderAsync(context);

        var rows = await context.OutboxMessages.AsNoTracking()
            .Where(x => x.AggregateId == poId.ToString())
            .ToListAsync();

        Assert.Single(rows);
        Assert.Equal("PurchaseOrderChanged", rows[0].EventType);
        Assert.Equal("PurchaseOrder", rows[0].AggregateType);
        Assert.Equal(poId.ToString(), rows[0].AggregateId);
        Assert.Null(rows[0].PublishedAtUtc);
        Assert.Equal(0, rows[0].Attempts);
        Assert.Contains("Created", rows[0].Payload);
    }

    [Fact]
    public async Task Marking_a_purchase_order_received_appends_a_PurchaseOrderReceived_row()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);

        var poId = await TestData.CreatePurchaseOrderAsync(context);
        await TestData.ClearOutboxAsync(context);

        var po = await context.PurchaseOrders.FirstAsync(x => x.Id == poId);
        po.MarkReceived(1);
        await context.SaveChangesAsync();

        var rows = await context.OutboxMessages.AsNoTracking()
            .Where(x => x.AggregateId == poId.ToString())
            .ToListAsync();

        Assert.Single(rows);
        Assert.Equal("PurchaseOrderReceived", rows[0].EventType);
        Assert.Contains("\"SupplierId\":1", rows[0].Payload);
    }
}
