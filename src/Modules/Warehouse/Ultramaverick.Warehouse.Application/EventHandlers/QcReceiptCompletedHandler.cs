using MediatR;
using Ultramaverick.SharedKernel.Outbox;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Services;
using Ultramaverick.Warehouse.Domain;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Application.EventHandlers;

public sealed class QcReceiptCompletedHandler : INotificationHandler<QcReceiptCompletedNotification>
{
    private readonly IWarehouseUnitOfWork _uow;
    private readonly InventoryLedger _ledger;
    private readonly IWarehouseSelector _selector;
    private readonly IProcessedEventStore _processedEvents;

    public QcReceiptCompletedHandler(
        IWarehouseUnitOfWork uow, InventoryLedger ledger, IWarehouseSelector selector,
        IProcessedEventStore processedEvents)
    {
        _uow = uow;
        _ledger = ledger;
        _selector = selector;
        _processedEvents = processedEvents;
    }

    public async Task Handle(QcReceiptCompletedNotification notification, CancellationToken ct)
    {
        // Guard domain state: a QC receipt already projected into stock is never re-projected.
        if (await _uow.Receipts.ExistsForQcReceiptAsync(notification.QcReceiptId, ct))
            return;

        // Consumer-owned idempotency: the marker and the receipt commit in one transaction,
        // so a retry after a partial failure can never double-produce receipts.
        if (await _processedEvents.HasProcessedAsync(notification.EventId, nameof(QcReceiptCompletedHandler), ct))
            return;

        // A zero (or negative) net quantity has no stock to project, but the event must still be
        // recorded as handled so redeliveries do not keep retrying it (ruling D3: no outbox poison).
        if (notification.NetQuantity <= 0)
        {
            await _processedEvents.MarkProcessedAsync(notification.EventId, nameof(QcReceiptCompletedHandler), ct);
            await _uow.SaveChangesAsync(ct);
            return;
        }

        var warehouseId = await _selector.DefaultForItemAsync(notification.ItemId, ct);

        var receipt = WarehouseReceipt.Create(
            qcReceiptId: notification.QcReceiptId,
            warehouseId: warehouseId,
            itemId: notification.ItemId,
            uomId: notification.UomId,
            lotNo: notification.LotNo,
            quantity: notification.NetQuantity,
            manufacturingDate: notification.ManufacturingDate,
            expiryDate: notification.ExpiryDate,
            receivedBy: notification.CompletedBy);

        await _uow.Receipts.AddAsync(receipt, ct);

        await _ledger.PostAsync(
            warehouseId, notification.ItemId, notification.UomId, notification.LotNo,
            notification.NetQuantity, MovementSource.QcReceipt,
            sourceDocumentId: notification.QcReceiptId, ct);

        await _processedEvents.MarkProcessedAsync(notification.EventId, nameof(QcReceiptCompletedHandler), ct);
        await _uow.SaveChangesAsync(ct);
    }
}
