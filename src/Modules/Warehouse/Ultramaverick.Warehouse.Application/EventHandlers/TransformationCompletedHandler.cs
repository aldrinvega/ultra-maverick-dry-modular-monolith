using MediatR;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Application.EventHandlers;

public sealed record TransformationCompletedNotification(
    int WarehouseId,
    IReadOnlyList<(int ItemId, decimal Quantity)> Lines,
    Guid EventId) : INotification;

/// <summary>Receives finished transformation lines into stock, once per outbox event.</summary>
public sealed class TransformationCompletedHandler
    : INotificationHandler<TransformationCompletedNotification>
{
    private const string Projection = nameof(TransformationCompletedHandler);

    private readonly IWarehouseUnitOfWork _uow;
    private readonly IItemUomLookup _uomLookup;
    private readonly IProcessedEventStore _processedEvents;

    public TransformationCompletedHandler(
        IWarehouseUnitOfWork uow,
        IItemUomLookup uomLookup,
        IProcessedEventStore processedEvents)
    {
        _uow = uow;
        _uomLookup = uomLookup;
        _processedEvents = processedEvents;
    }

    public async Task Handle(TransformationCompletedNotification notification, CancellationToken ct)
    {
        if (await _processedEvents.HasProcessedAsync(notification.EventId, Projection, ct))
            return;

        foreach (var line in notification.Lines)
        {
            var uomId = await _uomLookup.GetUomIdAsync(line.ItemId, ct)
                ?? throw new InvalidOperationException(
                    $"Item {line.ItemId} has no unit of measure in Catalog.");

            await _uow.Receipts.AddAsync(
                WarehouseReceipt.Create(
                    qcReceiptId: null,
                    warehouseId: notification.WarehouseId,
                    itemId: line.ItemId,
                    uomId: uomId,
                    lotNo: null,
                    quantity: line.Quantity,
                    manufacturingDate: null,
                    expiryDate: null,
                    receivedBy: null),
                ct);
        }

        await _processedEvents.MarkProcessedAsync(notification.EventId, Projection, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
