using System.Text.Json;
using MediatR;
using Ultramaverick.Orders.Contracts;
using Ultramaverick.Reporting.Application.EventHandlers;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.Infrastructure.Events;

/// <summary>Translates Orders' OrderCreated into Reporting's projection notification.</summary>
public sealed class OrderCreatedBridge : IIntegrationEventHandler
{
    private readonly IMediator _mediator;

    public OrderCreatedBridge(IMediator mediator) => _mediator = mediator;

    public string EventType => nameof(OrderCreated);

    public async Task HandleAsync(OutboxRecord record, CancellationToken ct)
    {
        var integrationEvent = JsonSerializer.Deserialize<OrderCreated>(record.Payload)
            ?? throw new InvalidOperationException(
                $"Could not deserialize {record.EventType} payload ({record.EventId}).");

        await _mediator.Publish(
            new OrderCreatedNotification(
                integrationEvent.OrderNumber,
                integrationEvent.CustomerId,
                integrationEvent.OccurredAtUtc,
                record.EventId),
            ct);
    }
}
