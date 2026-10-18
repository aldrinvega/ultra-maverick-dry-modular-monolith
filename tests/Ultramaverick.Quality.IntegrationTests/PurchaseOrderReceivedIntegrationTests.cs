using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ultramaverick.Identity.Infrastructure.BackgroundServices;
using Ultramaverick.Procurement.Contracts;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Quality.IntegrationTests;

/// <summary>
/// Proves the Procurement -> Quality integration end to end: a purchase order marked received
/// in Procurement writes a PurchaseOrderReceived outbox row, the shared dispatcher routes it to
/// Quality's bridge, and Quality projects one QC receipt per line. Redelivery is idempotent.
/// </summary>
public class PurchaseOrderReceivedIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PurchaseOrderReceivedIntegrationTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Marking_a_purchase_order_received_projects_one_receipt_per_line()
    {
        int purchaseOrderId;
        int[] lineIds;
        await using (var procurement = _fixture.CreateProcurementContext())
        {
            var po = Procurement.Domain.Entities.PurchaseOrder.Create(
                TestData.Code("PO"), null, null, null, supplierId: 5, null, null);
            po.AddLine(10, 20, 5m, 1m);
            po.AddLine(11, 21, 3m, 2m);
            procurement.PurchaseOrders.Add(po);
            await procurement.SaveChangesAsync();
            purchaseOrderId = po.Id;
            lineIds = po.Lines.Select(l => l.Id).OrderBy(x => x).ToArray();

            // Leave exactly the PurchaseOrderReceived row for the dispatcher to find.
            procurement.OutboxMessages.RemoveRange(procurement.OutboxMessages);
            await procurement.SaveChangesAsync();
        }

        await using (var procurement = _fixture.CreateProcurementContext())
        {
            // Include the lines: MarkReceived gathers their ids into the event, so without
            // them the PurchaseOrderReceived carries no line ids and Quality projects nothing.
            var po = await procurement.PurchaseOrders
                .Include(x => x.Lines)
                .FirstAsync(x => x.Id == purchaseOrderId);
            po.MarkReceived(1);
            await procurement.SaveChangesAsync();
        }

        await RunPublisherOnceAsync();

        await using (var quality = _fixture.CreateContext())
        {
            var receipts = await quality.QcReceipts.AsNoTracking()
                .Where(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync();

            Assert.Equal(2, receipts.Count);
            Assert.All(receipts, r => Assert.Equal("ForReceiving", r.Status));

            // Each receipt must carry the purchase-order line it was projected from.
            Assert.Equal(
                lineIds.Select(x => (int?)x).ToArray(),
                receipts.Select(r => r.PurchaseOrderLineId).OrderBy(x => x).ToArray());
        }
    }

    [Fact]
    public async Task The_bridge_is_idempotent_when_the_same_event_is_redelivered()
    {
        int purchaseOrderId, lineId;
        await using (var procurement = _fixture.CreateProcurementContext())
        {
            (purchaseOrderId, lineId) = await TestData.CreatePurchaseOrderWithLineAsync(procurement);
        }

        var payload = JsonSerializer.Serialize(new PurchaseOrderReceived(
            purchaseOrderId, SupplierId: 5, new[] { lineId }, DateTime.UtcNow));
        var record = new OutboxRecord(
            Guid.NewGuid(), "PurchaseOrder", purchaseOrderId.ToString(),
            "PurchaseOrderReceived", payload, Attempts: 0);

        using (var scope = _fixture.CreateScope())
        {
            var bridge = scope.ServiceProvider.GetServices<IIntegrationEventHandler>()
                .Single(h => h.GetType().FullName!.Contains(".Quality.") && h.EventType == "PurchaseOrderReceived");

            await bridge.HandleAsync(record, CancellationToken.None);
            // Redelivery of the very same event must not create duplicate receipts.
            await bridge.HandleAsync(record, CancellationToken.None);
        }

        await using (var quality = _fixture.CreateContext())
        {
            var receipts = await quality.QcReceipts.AsNoTracking()
                .Where(x => x.PurchaseOrderId == purchaseOrderId)
                .ToListAsync();

            Assert.Single(receipts);
        }
    }

    private async Task RunPublisherOnceAsync()
    {
        var publisher = new OutboxPublisherService(
            _fixture.Services.GetRequiredService<IServiceScopeFactory>(),
            _fixture.Services.GetRequiredService<ILogger<OutboxPublisherService>>());

        await publisher.RunOnceAsync(CancellationToken.None);
    }
}
