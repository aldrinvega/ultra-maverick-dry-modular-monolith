using Microsoft.EntityFrameworkCore;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.IntegrationTests;

/// <summary>
/// Proves the transactional outbox: a domain event raised on a Warehouse entity is written to
/// the module's Warehouse.OutboxMessages table in the same SaveChanges as the data change, so
/// the shared dispatcher can pick it up.
/// </summary>
public class OutboxTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OutboxTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Creating_a_warehouse_writes_one_WarehouseChanged_row()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);

        var warehouseId = await TestData.CreateWarehouseAsync(context);

        var rows = await context.OutboxMessages.AsNoTracking()
            .Where(x => x.AggregateId == warehouseId.ToString())
            .ToListAsync();

        Assert.Single(rows);
        Assert.Equal("WarehouseChanged", rows[0].EventType);
        Assert.Equal("Warehouse", rows[0].AggregateType);
        Assert.Equal(warehouseId.ToString(), rows[0].AggregateId);
        Assert.Null(rows[0].PublishedAtUtc);
        Assert.Equal(0, rows[0].Attempts);
        Assert.Contains("Created", rows[0].Payload);
    }

    [Fact]
    public async Task Creating_a_warehouse_receipt_appends_a_StockReceived_row()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);

        var receipt = WarehouseReceipt.Create(
            qcReceiptId: null, warehouseId: 1, itemId: 10, uomId: 20, lotNo: "LOT-1",
            quantity: 5m, manufacturingDate: null, expiryDate: null, receivedBy: 7);
        context.WarehouseReceipts.Add(receipt);
        await context.SaveChangesAsync();

        var rows = await context.OutboxMessages.AsNoTracking()
            .Where(x => x.EventType == "StockReceived")
            .ToListAsync();

        var row = Assert.Single(rows);
        Assert.Equal("WarehouseReceipt", row.AggregateType);
        Assert.Equal(receipt.Id.ToString(), row.AggregateId);
        Assert.Contains("\"Quantity\":5", row.Payload);
    }
}
