using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ultramaverick.Identity.Infrastructure.BackgroundServices;
using Ultramaverick.Quality.Persistence.Entities;

namespace Ultramaverick.Quality.IntegrationTests;

/// <summary>
/// Proves the negative liveness path: an outbox record whose EventType matches no registered
/// handler is still marked published (silent drop) but projects nothing.
/// </summary>
public class IntegrationLivenessTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public IntegrationLivenessTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task A_record_with_no_matching_handler_creates_no_qc_receipt()
    {
        const int purchaseOrderId = 987_650;
        var eventId = Guid.NewGuid();

        await using (var context = _fixture.CreateContext())
        {
            await TestData.ClearOutboxAsync(context);

            var record = OutboxMessage.Create(
                aggregateType: "PurchaseOrder",
                aggregateId: purchaseOrderId.ToString(),
                eventType: "NoSuchEvent",
                payload: $"{{\"PurchaseOrderId\":{purchaseOrderId}}}");
            context.OutboxMessages.Add(record);
            await context.SaveChangesAsync();
            eventId = record.EventId;
        }

        await RunPublisherOnceAsync();

        await using (var context = _fixture.CreateContext())
        {
            Assert.False(await context.QcReceipts.AsNoTracking()
                .AnyAsync(x => x.PurchaseOrderId == purchaseOrderId));

            var published = await context.OutboxMessages.AsNoTracking()
                .SingleAsync(m => m.EventId == eventId);
            Assert.NotNull(published.PublishedAtUtc);
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
