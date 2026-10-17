using Microsoft.EntityFrameworkCore;
using Ultramaverick.Procurement.Domain.Entities;

namespace Ultramaverick.Procurement.IntegrationTests;

/// <summary>
/// Proves the outbox write is transactional with the data change: when a purchase-order
/// insert fails inside SaveChanges, the outbox row raised for it is rolled back too, so the
/// dispatcher never publishes an event for data that was never persisted.
/// </summary>
public class OutboxRollbackTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OutboxRollbackTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task A_failed_purchase_order_insert_leaves_no_outbox_row()
    {
        var poNumber = $"PO{Guid.NewGuid():N}"[..12];

        await using (var context = _fixture.CreateContext())
        {
            context.PurchaseOrders.Add(PurchaseOrder.Create(poNumber, null, null, null, 1, null, null));
            await context.SaveChangesAsync();
        }

        await using (var context = _fixture.CreateContext())
        {
            await TestData.ClearOutboxAsync(context);
        }

        await using (var context = _fixture.CreateContext())
        {
            context.PurchaseOrders.Add(PurchaseOrder.Create(poNumber, null, null, null, 1, null, null));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        await using (var context = _fixture.CreateContext())
        {
            Assert.Empty(await context.OutboxMessages.AsNoTracking().ToListAsync());
        }
    }
}
