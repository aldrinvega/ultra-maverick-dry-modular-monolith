using System.Text.Json;
using MediatR;
using Ultramaverick.Reporting.Application.EventHandlers;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Contracts;

namespace Ultramaverick.Reporting.Infrastructure.Events;

/// <summary>Translates Warehouse's StockReceived into Reporting's projection notification.</summary>
public sealed class StockReceivedBridge : IIntegrationEventHandler
{
    private readonly IMediator _mediator;

    public StockReceivedBridge(IMediator mediator) => _mediator = mediator;

    public string EventType => nameof(StockReceived);

    public async Task HandleAsync(OutboxRecord record, CancellationToken ct)
    {
        var integrationEvent = JsonSerializer.Deserialize<StockReceived>(record.Payload)
            ?? throw new InvalidOperationException(
                $"Could not deserialize {record.EventType} payload ({record.EventId}).");

        await _mediator.Publish(
            new StockReceivedNotification(
                integrationEvent.WarehouseId,
                integrationEvent.ItemId,
                integrationEvent.UomId,
                integrationEvent.LotNo,
                integrationEvent.Quantity,
                integrationEvent.OccurredAtUtc,
                record.EventId),
            ct);
    }
}
