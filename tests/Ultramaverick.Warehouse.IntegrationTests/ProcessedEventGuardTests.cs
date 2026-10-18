using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.IntegrationTests;

/// <summary>
/// Isolates the consumer-owned processed-event guard: a redelivery of an already-handled event
/// id must not produce a second receipt or a second balance movement, and the business guard
/// (a receipt already exists for the QC receipt) blocks a re-projection without committing.
/// </summary>
public class ProcessedEventGuardTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ProcessedEventGuardTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task A_redelivery_of_the_same_event_id_is_blocked_by_the_marker()
    {
        await using (var context = _fixture.CreateContext())
        {
            await TestData.CreateWarehouseAsync(context);
        }

        var eventId = Guid.NewGuid();
        var record = TestData.QcReceiptCompletedRecord(
            eventId, qcReceiptId: 990, itemId: 11, uomId: 21, lotNo: null, netQuantity: 3m);

        using (var scope = _fixture.CreateScope())
        {
            await ResolveBridge(scope).HandleAsync(record, CancellationToken.None);
        }

        using (var scope = _fixture.CreateScope())
        {
            await ResolveBridge(scope).HandleAsync(record, CancellationToken.None);
        }

        await using (var context = _fixture.CreateContext())
        {
            Assert.True(await context.ProcessedEvents.AsNoTracking()
                .AnyAsync(p => p.EventId == eventId));
            Assert.Equal(1, await context.WarehouseReceipts.AsNoTracking()
                .CountAsync(r => r.QcReceiptId == 990));

            var balance = await context.InventoryBalances.AsNoTracking()
                .SingleAsync(b => b.ItemId == 11);
            Assert.Equal(3m, balance.OnHand);
        }
    }

    [Fact]
    public async Task The_business_guard_blocks_a_second_receipt_for_the_same_qc_receipt()
    {
        await using (var context = _fixture.CreateContext())
        {
            var warehouseId = await TestData.CreateWarehouseAsync(context);

            var existing = WarehouseReceipt.Create(
                qcReceiptId: 500, warehouseId: warehouseId, itemId: 12, uomId: 22, lotNo: null,
                quantity: 1m, manufacturingDate: null, expiryDate: null, receivedBy: null);
            context.WarehouseReceipts.Add(existing);
            await context.SaveChangesAsync();
        }

        var eventId = Guid.NewGuid();
        var record = TestData.QcReceiptCompletedRecord(
            eventId, qcReceiptId: 500, itemId: 12, uomId: 22, lotNo: null, netQuantity: 4m);

        using (var scope = _fixture.CreateScope())
        {
            await ResolveBridge(scope).HandleAsync(record, CancellationToken.None);
        }

        await using (var context = _fixture.CreateContext())
        {
            Assert.Equal(1, await context.WarehouseReceipts.AsNoTracking()
                .CountAsync(r => r.QcReceiptId == 500));
            Assert.False(await context.ProcessedEvents.AsNoTracking()
                .AnyAsync(p => p.EventId == eventId));
        }
    }

    private static IIntegrationEventHandler ResolveBridge(IServiceScope scope)
        => scope.ServiceProvider.GetServices<IIntegrationEventHandler>()
            .Single(h => h.GetType().FullName!.Contains(".Warehouse.") && h.EventType == "QcReceiptCompleted");
}
