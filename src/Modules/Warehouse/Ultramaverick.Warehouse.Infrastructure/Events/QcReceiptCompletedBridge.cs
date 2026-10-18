using System.Text.Json;
using MediatR;
using Ultramaverick.Quality.Contracts;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Application.EventHandlers;

namespace Ultramaverick.Warehouse.Infrastructure.Events;

/// <summary>
/// Translates Quality's published QcReceiptCompleted integration event into Warehouse's
/// in-process notification. The outbox row's EventId rides along so the handler can apply its
/// consumer-owned idempotency guard. Exceptions are intentionally not swallowed — the outbox
/// publisher records a failure and retries, and the handler is idempotent.
/// </summary>
public sealed class QcReceiptCompletedBridge : IIntegrationEventHandler
{
    private readonly IMediator _mediator;

    public QcReceiptCompletedBridge(IMediator mediator) => _mediator = mediator;

    public string EventType => nameof(QcReceiptCompleted);

    public async Task HandleAsync(OutboxRecord record, CancellationToken ct)
    {
        // Default options: the producer serialises with the same default (PascalCase) options.
        var integrationEvent = JsonSerializer.Deserialize<QcReceiptCompleted>(record.Payload)
            ?? throw new InvalidOperationException(
                $"Could not deserialize {record.EventType} payload ({record.EventId}).");

        await _mediator.Publish(
            new QcReceiptCompletedNotification(
                integrationEvent.QcReceiptId,
                integrationEvent.ItemId,
                integrationEvent.UomId,
                integrationEvent.LotNo,
                integrationEvent.NetQuantity,
                integrationEvent.ManufacturingDate,
                integrationEvent.ExpiryDate,
                integrationEvent.CompletedBy,
                record.EventId),
            ct);
    }
}
