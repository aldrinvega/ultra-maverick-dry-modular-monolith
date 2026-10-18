using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Procurement.Contracts;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Quality.IntegrationTests;

/// <summary>
/// Isolates the consumer-owned processed-event guard: a first delivery with no line ids still
/// commits the processed marker, so a redelivery of the same EventId with a real line stays a
/// no-op even though no receipt exists for the purchase order.
/// </summary>
public class ProcessedEventGuardTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ProcessedEventGuardTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task An_empty_line_set_still_commits_the_marker_so_redelivery_is_blocked()
    {
        int purchaseOrderId, lineId;
        await using (var procurement = _fixture.CreateProcurementContext())
        {
            (purchaseOrderId, lineId) = await TestData.CreatePurchaseOrderWithLineAsync(procurement);
        }

        var eventId = Guid.NewGuid();

        var emptyRecord = new OutboxRecord(
            eventId, "PurchaseOrder", purchaseOrderId.ToString(), "PurchaseOrderReceived",
            JsonSerializer.Serialize(new PurchaseOrderReceived(
                purchaseOrderId, SupplierId: 5, Array.Empty<int>(), DateTime.UtcNow)),
            Attempts: 0);

        using (var scope = _fixture.CreateScope())
        {
            await ResolveBridge(scope).HandleAsync(emptyRecord, CancellationToken.None);
        }

        await using (var quality = _fixture.CreateContext())
        {
            Assert.True(await quality.ProcessedEvents.AsNoTracking()
                .AnyAsync(p => p.EventId == eventId));
            Assert.False(await quality.QcReceipts.AsNoTracking()
                .AnyAsync(x => x.PurchaseOrderId == purchaseOrderId));
        }

        var realRecord = emptyRecord with
        {
            Payload = JsonSerializer.Serialize(new PurchaseOrderReceived(
                purchaseOrderId, SupplierId: 5, new[] { lineId }, DateTime.UtcNow))
        };

        using (var scope = _fixture.CreateScope())
        {
            await ResolveBridge(scope).HandleAsync(realRecord, CancellationToken.None);
        }

        await using (var quality = _fixture.CreateContext())
        {
            Assert.False(await quality.QcReceipts.AsNoTracking()
                .AnyAsync(x => x.PurchaseOrderId == purchaseOrderId));
        }
    }

    private static IIntegrationEventHandler ResolveBridge(IServiceScope scope)
        => scope.ServiceProvider.GetServices<IIntegrationEventHandler>()
            .Single(h => h.GetType().FullName!.Contains(".Quality.") && h.EventType == "PurchaseOrderReceived");
}
