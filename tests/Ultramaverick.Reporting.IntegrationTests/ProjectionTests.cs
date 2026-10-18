using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Quality.Contracts;
using Ultramaverick.Orders.Contracts;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Contracts;

namespace Ultramaverick.Reporting.IntegrationTests;

/// <summary>
/// Proves the projections end to end: a published outbox record is handled by the registered
/// bridge, written to the Reporting read model, and ignored when the same event id arrives again.
/// </summary>
public class ProjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ProjectionTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Stock_received_projects_into_inventory_positions_and_is_idempotent()
    {
        var eventId = Guid.NewGuid();
        var lotNo = $"LOT-{Guid.NewGuid():N}"[..16];
        var payload = JsonSerializer.Serialize(
            new StockReceived(1, 10, 20, lotNo, 5m, DateTime.UtcNow));

        var record = new OutboxRecord(eventId, "WarehouseReceipt", "1", nameof(StockReceived), payload, 0);

        await DispatchAsync(nameof(StockReceived), record);
        await DispatchAsync(nameof(StockReceived), record);

        await using var context = _fixture.CreateContext();
        var position = await context.InventoryPositions.AsNoTracking()
            .SingleAsync(x => x.WarehouseId == 1 && x.ItemId == 10 && x.LotNo == lotNo);

        Assert.Equal(5m, position.Quantity);
    }

    [Fact]
    public async Task The_order_lifecycle_projects_the_latest_status()
    {
        var orderNumber = $"SO-{Guid.NewGuid():N}"[..20];

        await DispatchAsync(
            nameof(OrderCreated),
            Record(nameof(OrderCreated), JsonSerializer.Serialize(
                new OrderCreated(orderNumber, 7, DateTime.UtcNow))));

        await DispatchAsync(
            nameof(OrderConfirmed),
            Record(nameof(OrderConfirmed), JsonSerializer.Serialize(
                new OrderConfirmed(orderNumber, DateTime.UtcNow))));

        await using var context = _fixture.CreateContext();
        var row = await context.OrderFulfillment.AsNoTracking()
            .SingleAsync(x => x.OrderNumber == orderNumber);

        Assert.Equal("Confirmed", row.Status);
        Assert.Equal(7, row.CustomerId);
    }

    [Fact]
    public async Task A_completed_qc_receipt_projects_into_qc_status()
    {
        var qcReceiptId = Random.Shared.Next(1_000, 9_999);

        await DispatchAsync(
            nameof(QcReceiptCompleted),
            Record(nameof(QcReceiptCompleted), JsonSerializer.Serialize(
                new QcReceiptCompleted(qcReceiptId, 10, 20, "LOT-Q1", 50m, null, null, 7, DateTime.UtcNow))));

        await using var context = _fixture.CreateContext();
        var row = await context.QcStatus.AsNoTracking()
            .SingleAsync(x => x.QcReceiptId == qcReceiptId);

        Assert.Equal(50m, row.NetQuantity);
        Assert.Equal("Completed", row.Status);
    }

    private static OutboxRecord Record(string eventType, string payload)
        => new(Guid.NewGuid(), "Test", "1", eventType, payload, 0);

    private async Task DispatchAsync(string eventType, OutboxRecord record)
    {
        using var scope = _fixture.CreateScope();
        var handler = scope.ServiceProvider.GetServices<IIntegrationEventHandler>()
            .Single(x => x.EventType == eventType);

        await handler.HandleAsync(record, CancellationToken.None);
    }
}
