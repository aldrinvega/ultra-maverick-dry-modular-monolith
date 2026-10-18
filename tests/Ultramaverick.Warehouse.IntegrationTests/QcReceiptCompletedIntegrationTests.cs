using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Domain;

namespace Ultramaverick.Warehouse.IntegrationTests;

/// <summary>
/// End-to-end path for the one cross-module surface Warehouse consumes: the Quality bridge turns
/// a QcReceiptCompleted outbox record into a warehouse receipt, an inventory balance, an
/// append-only movement, a processed marker and a fresh StockReceived outbox row.
/// </summary>
public class QcReceiptCompletedIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public QcReceiptCompletedIntegrationTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task The_bridge_receipts_stock_and_posts_a_balance_and_movement()
    {
        int warehouseId;
        await using (var context = _fixture.CreateContext())
        {
            warehouseId = await TestData.CreateWarehouseAsync(context);
            await TestData.ClearOutboxAsync(context);
        }

        var qcReceiptId = 4242;
        var eventId = Guid.NewGuid();
        var record = TestData.QcReceiptCompletedRecord(
            eventId, qcReceiptId, itemId: 10, uomId: 20, lotNo: "LOT-X", netQuantity: 7m);

        using (var scope = _fixture.CreateScope())
        {
            await ResolveBridge(scope).HandleAsync(record, CancellationToken.None);
        }

        await using (var context = _fixture.CreateContext())
        {
            var receipt = await context.WarehouseReceipts.AsNoTracking()
                .SingleAsync(r => r.QcReceiptId == qcReceiptId);
            Assert.Equal(warehouseId, receipt.WarehouseId);
            Assert.Equal(7m, receipt.Quantity);
            Assert.Equal("LOT-X", receipt.LotNo);

            var balance = await context.InventoryBalances.AsNoTracking()
                .SingleAsync(b => b.WarehouseId == warehouseId && b.ItemId == 10);
            Assert.Equal(7m, balance.OnHand);

            var movement = await context.InventoryMovements.AsNoTracking()
                .SingleAsync(m => m.WarehouseId == warehouseId && m.ItemId == 10);
            Assert.Equal(MovementSource.QcReceipt, movement.Source);
            Assert.Equal(qcReceiptId, movement.SourceDocumentId);
            Assert.Equal(7m, movement.Delta);

            Assert.True(await context.ProcessedEvents.AsNoTracking()
                .AnyAsync(p => p.EventId == eventId));

            // The receipt raised StockReceived, written to Warehouse's own outbox.
            Assert.True(await context.OutboxMessages.AsNoTracking()
                .AnyAsync(o => o.EventType == "StockReceived"));
        }
    }

    [Fact]
    public async Task The_bridge_records_a_non_positive_quantity_event_without_producing_stock()
    {
        await using (var context = _fixture.CreateContext())
        {
            await TestData.ClearOutboxAsync(context);
        }

        var qcReceiptId = 4343;
        var eventId = Guid.NewGuid();
        var record = TestData.QcReceiptCompletedRecord(
            eventId, qcReceiptId, itemId: 13, uomId: 23, lotNo: "LOT-Z", netQuantity: 0m);

        using (var scope = _fixture.CreateScope())
        {
            await ResolveBridge(scope).HandleAsync(record, CancellationToken.None);
        }

        await using (var context = _fixture.CreateContext())
        {
            // (a) The marker still commits, so a redelivery is never retried as poison.
            Assert.True(await context.ProcessedEvents.AsNoTracking()
                .AnyAsync(p => p.EventId == eventId));

            // (b)/(c) No stock is projected: no receipt, no balance and no movement.
            Assert.False(await context.WarehouseReceipts.AsNoTracking()
                .AnyAsync(r => r.QcReceiptId == qcReceiptId));
            Assert.False(await context.InventoryBalances.AsNoTracking()
                .AnyAsync(b => b.ItemId == 13));
            Assert.False(await context.InventoryMovements.AsNoTracking()
                .AnyAsync(m => m.ItemId == 13));

            // (d) With no receipt there is no StockReceived row in Warehouse's outbox.
            Assert.False(await context.OutboxMessages.AsNoTracking()
                .AnyAsync(o => o.EventType == "StockReceived"));
        }
    }

    [Fact]
    public void The_bridge_preserves_the_event_type_literal_quality_writes()
    {
        using var scope = _fixture.CreateScope();

        Assert.Equal("QcReceiptCompleted", ResolveBridge(scope).EventType);
    }

    [Fact]
    public async Task HandleAsync_propagates_a_malformed_payload()
    {
        using var scope = _fixture.CreateScope();
        var record = new OutboxRecord(
            Guid.NewGuid(), "QcReceipt", "1", "QcReceiptCompleted", "{ not-valid-json", Attempts: 0);

        await Assert.ThrowsAnyAsync<Exception>(
            () => ResolveBridge(scope).HandleAsync(record, CancellationToken.None));
    }

    private static IIntegrationEventHandler ResolveBridge(IServiceScope scope)
        => scope.ServiceProvider.GetServices<IIntegrationEventHandler>()
            .Single(h => h.GetType().FullName!.Contains(".Warehouse.") && h.EventType == "QcReceiptCompleted");
}
