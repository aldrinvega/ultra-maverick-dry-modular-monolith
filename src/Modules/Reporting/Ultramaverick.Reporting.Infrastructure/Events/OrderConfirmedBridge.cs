using System.Text.Json;
using MediatR;
using Ultramaverick.Orders.Contracts;
using Ultramaverick.Reporting.Application.EventHandlers;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.Infrastructure.Events;

/// <summary>Translates Orders' OrderConfirmed into Reporting's projection notification.</summary>
public sealed class OrderConfirmedBridge : IIntegrationEventHandler
{
    private readonly IMediator _mediator;

    public OrderConfirmedBridge(IMediator mediator) => _mediator = mediator;

    public string EventType => nameof(OrderConfirmed);

    public async Task HandleAsync(OutboxRecord record, CancellationToken ct)
    {
        var integrationEvent = JsonSerializer.Deserialize<OrderConfirmed>(record.Payload)
            ?? throw new InvalidOperationException(
                $"Could not deserialize {record.EventType} payload ({record.EventId}).");

        await _mediator.Publish(
            new OrderConfirmedNotification(
                integrationEvent.OrderNumber,
                integrationEvent.OccurredAtUtc,
                record.EventId),
            ct);
    }
}
