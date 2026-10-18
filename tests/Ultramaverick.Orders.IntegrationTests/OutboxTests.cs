using Microsoft.EntityFrameworkCore;

namespace Ultramaverick.Orders.IntegrationTests;

/// <summary>
/// Proves the transactional outbox: the <c>OrderCreated</c> event raised by <c>Order.Create</c>
/// is written to the Ordering.OutboxMessages table in the same SaveChanges as the order row, so
/// the shared dispatcher can pick it up.
/// </summary>
public class OutboxTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OutboxTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Creating_an_order_writes_one_OrderCreated_row()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);

        var orderId = await TestData.CreateOrderAsync(context);

        var rows = await context.OutboxMessages.AsNoTracking()
            .Where(x => x.AggregateId == orderId.ToString())
            .ToListAsync();

        var row = Assert.Single(rows);
        Assert.Equal("OrderCreated", row.EventType);
        Assert.Equal("Order", row.AggregateType);
        Assert.Equal(orderId.ToString(), row.AggregateId);
        Assert.Null(row.PublishedAtUtc);
        Assert.Equal(0, row.Attempts);
        Assert.Contains("OrderNumber", row.Payload);

        var persistedLines = await context.OrderLines.AsNoTracking()
            .Where(x => x.OrderId == orderId)
            .ToListAsync();

        var line = Assert.Single(persistedLines);
        Assert.Equal(10, line.ItemId);
        Assert.Equal(5m, line.QuantityOrdered);
    }
}
