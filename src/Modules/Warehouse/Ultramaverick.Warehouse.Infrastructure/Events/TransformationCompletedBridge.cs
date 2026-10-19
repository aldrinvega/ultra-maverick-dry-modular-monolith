using System.Text.Json;
using MediatR;
using Ultramaverick.Manufacturing.Contracts;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Application.EventHandlers;

namespace Ultramaverick.Warehouse.Infrastructure.Events;

/// <summary>Translates Manufacturing's TransformationCompleted into Warehouse's notification.</summary>
public sealed class TransformationCompletedBridge : IIntegrationEventHandler
{
    private readonly IMediator _mediator;

    public TransformationCompletedBridge(IMediator mediator) => _mediator = mediator;

    public string EventType => nameof(TransformationCompleted);

    public async Task HandleAsync(OutboxRecord record, CancellationToken ct)
    {
        var integrationEvent = JsonSerializer.Deserialize<TransformationCompleted>(record.Payload)
            ?? throw new InvalidOperationException(
                $"Could not deserialize {record.EventType} payload ({record.EventId}).");

        var lines = integrationEvent.Lines
            .Select(line => (line.ItemId, line.Quantity))
            .ToList();

        await _mediator.Publish(
            new TransformationCompletedNotification(
                integrationEvent.WarehouseId, lines, record.EventId),
            ct);
    }
}
