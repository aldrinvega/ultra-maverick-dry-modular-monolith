using MediatR;
using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Domain.Entities;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.Application.EventHandlers;

public sealed record OrderConfirmedNotification(
    string OrderNumber,
    DateTime OccurredAtUtc,
    Guid EventId) : INotification;

/// <summary>Projects Orders' OrderConfirmed into Reporting.OrderFulfillment.</summary>
public sealed class OrderConfirmedHandler : INotificationHandler<OrderConfirmedNotification>
{
    private const string Projection = nameof(OrderConfirmedHandler);

    private readonly IReportingUnitOfWork _uow;
    private readonly IProcessedEventStore _processedEvents;

    public OrderConfirmedHandler(IReportingUnitOfWork uow, IProcessedEventStore processedEvents)
    {
        _uow = uow;
        _processedEvents = processedEvents;
    }

    public async Task Handle(OrderConfirmedNotification notification, CancellationToken ct)
    {
        if (await _processedEvents.HasProcessedAsync(notification.EventId, Projection, ct))
            return;

        var fulfillment = await _uow.OrderFulfillment.FindAsync(notification.OrderNumber, ct);

        if (fulfillment is null)
        {
            fulfillment = OrderFulfillment.Create(notification.OrderNumber, null, notification.OccurredAtUtc);
            await _uow.OrderFulfillment.AddAsync(fulfillment, ct);
        }

        fulfillment.Confirm(notification.OccurredAtUtc);

        await _processedEvents.MarkProcessedAsync(notification.EventId, Projection, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
