using System.Text.Json;
using MediatR;
using Ultramaverick.Orders.Contracts;
using Ultramaverick.Reporting.Application.EventHandlers;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.Infrastructure.Events;

/// <summary>Translates Orders' OrderCancelled into Reporting's projection notification.</summary>
public sealed class OrderCancelledBridge : IIntegrationEventHandler
{
    private readonly IMediator _mediator;

    public OrderCancelledBridge(IMediator mediator) => _mediator = mediator;

    public string EventType => nameof(OrderCancelled);

    public async Task HandleAsync(OutboxRecord record, CancellationToken ct)
    {
        var integrationEvent = JsonSerializer.Deserialize<OrderCancelled>(record.Payload)
            ?? throw new InvalidOperationException(
                $"Could not deserialize {record.EventType} payload ({record.EventId}).");

        await _mediator.Publish(
            new OrderCancelledNotification(
                integrationEvent.OrderNumber,
                integrationEvent.OccurredAtUtc,
                record.EventId),
            ct);
    }
}
