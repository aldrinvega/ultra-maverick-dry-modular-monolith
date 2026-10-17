using Microsoft.EntityFrameworkCore;
using Ultramaverick.Procurement.Domain.Entities;

namespace Ultramaverick.Procurement.IntegrationTests;

public class PurchaseOrderPersistenceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PurchaseOrderPersistenceTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Lines_round_trip_with_decimal_precision()
    {
        int poId;
        await using (var context = _fixture.CreateContext())
        {
            var po = PurchaseOrder.Create(Code("PO"), null, null, null, 1, null, null);
            po.AddLine(10, 20, 3.1234m, 9.8765m);
            context.PurchaseOrders.Add(po);
            await context.SaveChangesAsync();
            poId = po.Id;
        }

        await using (var context = _fixture.CreateContext())
        {
            var reloaded = await context.PurchaseOrders
                .Include(x => x.Lines)
                .SingleAsync(x => x.Id == poId);

            var line = Assert.Single(reloaded.Lines);
            Assert.Equal(10, line.ItemId);
            Assert.Equal(20, line.UomId);
            Assert.Equal(3.1234m, line.OrderedQuantity);
            Assert.Equal(9.8765m, line.UnitPrice);
        }
    }

    [Fact]
    public async Task Duplicate_po_number_is_rejected_by_the_unique_index()
    {
        var poNumber = Code("PO");

        await using (var context = _fixture.CreateContext())
        {
            context.PurchaseOrders.Add(PurchaseOrder.Create(poNumber, null, null, null, 1, null, null));
            await context.SaveChangesAsync();
        }

        await using (var context = _fixture.CreateContext())
        {
            context.PurchaseOrders.Add(PurchaseOrder.Create(poNumber, null, null, null, 1, null, null));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task MarkReceived_twice_persists_once_and_raises_one_event()
    {
        int poId;
        await using (var context = _fixture.CreateContext())
        {
            poId = await TestData.CreatePurchaseOrderAsync(context);
        }

        await using (var context = _fixture.CreateContext())
        {
            await TestData.ClearOutboxAsync(context);

            var po = await context.PurchaseOrders.FirstAsync(x => x.Id == poId);
            po.MarkReceived(1);
            po.MarkReceived(1);
            await context.SaveChangesAsync();
        }

        await using (var context = _fixture.CreateContext())
        {
            var po = await context.PurchaseOrders.AsNoTracking().SingleAsync(x => x.Id == poId);
            Assert.Equal("Received", po.Status);

            var rows = await context.OutboxMessages.AsNoTracking()
                .Where(x => x.AggregateId == poId.ToString() && x.EventType == "PurchaseOrderReceived")
                .ToListAsync();

            Assert.Single(rows);
        }
    }

    private static string Code(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..12];
}
