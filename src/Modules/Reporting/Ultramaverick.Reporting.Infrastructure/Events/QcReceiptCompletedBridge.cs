using System.Text.Json;
using MediatR;
using Ultramaverick.Quality.Contracts;
using Ultramaverick.Reporting.Application.EventHandlers;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.Infrastructure.Events;

/// <summary>Translates Quality's QcReceiptCompleted into Reporting's projection notification.</summary>
public sealed class QcReceiptCompletedBridge : IIntegrationEventHandler
{
    private readonly IMediator _mediator;

    public QcReceiptCompletedBridge(IMediator mediator) => _mediator = mediator;

    public string EventType => nameof(QcReceiptCompleted);

    public async Task HandleAsync(OutboxRecord record, CancellationToken ct)
    {
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
                integrationEvent.OccurredAtUtc,
                record.EventId),
            ct);
    }
}
