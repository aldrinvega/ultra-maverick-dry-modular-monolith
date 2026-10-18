using MediatR;
using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Domain.Entities;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.Application.EventHandlers;

public sealed record StockReceivedNotification(
    int WarehouseId,
    int ItemId,
    int UomId,
    string? LotNo,
    decimal Quantity,
    DateTime OccurredAtUtc,
    Guid EventId) : INotification;

/// <summary>Projects Warehouse's StockReceived into Reporting.InventoryPositions.</summary>
public sealed class StockReceivedHandler : INotificationHandler<StockReceivedNotification>
{
    private const string Projection = nameof(StockReceivedHandler);

    private readonly IReportingUnitOfWork _uow;
    private readonly IProcessedEventStore _processedEvents;

    public StockReceivedHandler(IReportingUnitOfWork uow, IProcessedEventStore processedEvents)
    {
        _uow = uow;
        _processedEvents = processedEvents;
    }

    public async Task Handle(StockReceivedNotification notification, CancellationToken ct)
    {
        if (await _processedEvents.HasProcessedAsync(notification.EventId, Projection, ct))
            return;

        var lotNo = InventoryPosition.NormalizeLot(notification.LotNo);

        var position = await _uow.InventoryPositions
            .FindAsync(notification.WarehouseId, notification.ItemId, lotNo, ct);

        if (position is null)
        {
            position = InventoryPosition.Create(
                notification.WarehouseId,
                notification.ItemId,
                notification.UomId,
                notification.LotNo,
                notification.OccurredAtUtc);

            await _uow.InventoryPositions.AddAsync(position, ct);
        }

        position.AddOnHand(notification.Quantity, notification.OccurredAtUtc);

        await _processedEvents.MarkProcessedAsync(notification.EventId, Projection, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
