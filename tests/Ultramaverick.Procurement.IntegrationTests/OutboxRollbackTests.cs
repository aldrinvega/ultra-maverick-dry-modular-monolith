using Microsoft.EntityFrameworkCore;
using Ultramaverick.Procurement.Domain.Entities;

namespace Ultramaverick.Procurement.IntegrationTests;

/// <summary>
/// Proves the purchase-order insert and its outbox write are a single atomic unit of work:
/// when the outbox flush fails after the purchase order has already been inserted, the
/// whole unit rolls back, so neither the purchase order nor an outbox row is persisted and
/// the dispatcher never publishes an event for data that was never committed.
/// </summary>
public class OutboxRollbackTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OutboxRollbackTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task A_failed_outbox_flush_rolls_back_the_purchase_order_insert()
    {
        var poNumber = $"PO{Guid.NewGuid():N}"[..12];

        await using (var context = _fixture.CreateContext())
        {
            await TestData.ClearOutboxAsync(context);
        }

        await using (var context = _fixture.CreateContext(new FailingOutboxCommandInterceptor()))
        {
            context.PurchaseOrders.Add(PurchaseOrder.Create(poNumber, null, null, null, 1, null, null));
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            Assert.IsType<InvalidOperationException>(exception.InnerException);
        }

        await using (var context = _fixture.CreateContext())
        {
            Assert.Empty(await context.PurchaseOrders.AsNoTracking()
                .Where(x => x.PoNumber == poNumber).ToListAsync());
            Assert.Empty(await context.OutboxMessages.AsNoTracking().ToListAsync());
        }
    }
}
