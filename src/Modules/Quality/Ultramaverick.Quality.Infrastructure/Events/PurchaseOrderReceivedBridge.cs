using System.Text.Json;
using MediatR;
using Ultramaverick.Procurement.Contracts;
using Ultramaverick.Quality.Application.EventHandlers;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Quality.Infrastructure.Events;

/// <summary>
/// Translates Procurement's published PurchaseOrderReceived integration event into Quality's
/// in-process notification. The outbox row's EventId rides along so the handler can apply its
/// consumer-owned idempotency guard. Exceptions are intentionally not swallowed — the outbox
/// publisher records a failure and retries, and the handler is idempotent.
/// </summary>
public sealed class PurchaseOrderReceivedBridge : IIntegrationEventHandler
{
    private readonly IMediator _mediator;

    public PurchaseOrderReceivedBridge(IMediator mediator) => _mediator = mediator;

    public string EventType => nameof(PurchaseOrderReceived);

    public async Task HandleAsync(OutboxRecord record, CancellationToken ct)
    {
        // Default options: the producer serialises with the same default (PascalCase) options.
        var integrationEvent = JsonSerializer.Deserialize<PurchaseOrderReceived>(record.Payload)
            ?? throw new InvalidOperationException(
                $"Could not deserialize {record.EventType} payload ({record.EventId}).");

        await _mediator.Publish(
            new PurchaseOrderReceivedNotification(
                integrationEvent.PurchaseOrderId,
                integrationEvent.SupplierId,
                integrationEvent.LineItemIds,
                record.EventId),
            ct);
    }
}
