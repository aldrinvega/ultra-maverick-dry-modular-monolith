using MediatR;
using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Domain.Entities;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.Application.EventHandlers;

public sealed record OrderCreatedNotification(
    string OrderNumber,
    int CustomerId,
    DateTime OccurredAtUtc,
    Guid EventId) : INotification;

/// <summary>Projects Orders' OrderCreated into Reporting.OrderFulfillment.</summary>
public sealed class OrderCreatedHandler : INotificationHandler<OrderCreatedNotification>
{
    private const string Projection = nameof(OrderCreatedHandler);

    private readonly IReportingUnitOfWork _uow;
    private readonly IProcessedEventStore _processedEvents;

    public OrderCreatedHandler(IReportingUnitOfWork uow, IProcessedEventStore processedEvents)
    {
        _uow = uow;
        _processedEvents = processedEvents;
    }

    public async Task Handle(OrderCreatedNotification notification, CancellationToken ct)
    {
        if (await _processedEvents.HasProcessedAsync(notification.EventId, Projection, ct))
            return;

        var fulfillment = await _uow.OrderFulfillment.FindAsync(notification.OrderNumber, ct);

        if (fulfillment is null)
        {
            await _uow.OrderFulfillment.AddAsync(
                OrderFulfillment.Create(notification.OrderNumber, notification.CustomerId, notification.OccurredAtUtc),
                ct);
        }

        await _processedEvents.MarkProcessedAsync(notification.EventId, Projection, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
