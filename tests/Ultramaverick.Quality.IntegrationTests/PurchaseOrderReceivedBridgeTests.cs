using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Quality.IntegrationTests;

/// <summary>
/// Pins the Quality bridge's contract with the outbox: it must surface failures (never swallow)
/// and its EventType literal must match what the Procurement producer writes.
/// </summary>
public class PurchaseOrderReceivedBridgeTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PurchaseOrderReceivedBridgeTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task HandleAsync_propagates_a_malformed_payload()
    {
        using var scope = _fixture.CreateScope();
        var record = new OutboxRecord(
            Guid.NewGuid(), "PurchaseOrder", "1", "PurchaseOrderReceived", "{ not-valid-json", Attempts: 0);

        await Assert.ThrowsAnyAsync<Exception>(
            () => ResolveBridge(scope).HandleAsync(record, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_propagates_a_null_payload()
    {
        using var scope = _fixture.CreateScope();
        var record = new OutboxRecord(
            Guid.NewGuid(), "PurchaseOrder", "1", "PurchaseOrderReceived", null!, Attempts: 0);

        await Assert.ThrowsAnyAsync<Exception>(
            () => ResolveBridge(scope).HandleAsync(record, CancellationToken.None));
    }

    [Fact]
    public async Task The_bridge_EventType_equals_the_literal_the_procurement_producer_writes()
    {
        int purchaseOrderId;
        await using (var procurement = _fixture.CreateProcurementContext())
        {
            (purchaseOrderId, _) = await TestData.CreatePurchaseOrderWithLineAsync(procurement);
        }

        string producedEventType;
        await using (var procurement = _fixture.CreateProcurementContext())
        {
            var po = await procurement.PurchaseOrders
                .Include(x => x.Lines)
                .FirstAsync(x => x.Id == purchaseOrderId);
            po.MarkReceived(1);
            await procurement.SaveChangesAsync();

            producedEventType = await procurement.OutboxMessages.AsNoTracking()
                .Where(m => m.EventType == "PurchaseOrderReceived")
                .Select(m => m.EventType)
                .SingleAsync();
        }

        using var scope = _fixture.CreateScope();

        Assert.Equal("PurchaseOrderReceived", producedEventType);
        Assert.Equal(producedEventType, ResolveBridge(scope).EventType);
    }

    private static IIntegrationEventHandler ResolveBridge(IServiceScope scope)
        => scope.ServiceProvider.GetServices<IIntegrationEventHandler>()
            .Single(h => h.GetType().FullName!.Contains(".Quality.") && h.EventType == "PurchaseOrderReceived");
}
