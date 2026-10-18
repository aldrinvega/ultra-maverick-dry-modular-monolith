using MediatR;
using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Domain.Entities;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Quality.Application.EventHandlers;

public sealed class PurchaseOrderReceivedHandler : INotificationHandler<PurchaseOrderReceivedNotification>
{
    private readonly IQualityUnitOfWork _uow;
    private readonly IPurchaseOrderLineLookup _lines;   // query port into Procurement/Catalog
    private readonly IProcessedEventStore _processedEvents;

    public PurchaseOrderReceivedHandler(
        IQualityUnitOfWork uow, IPurchaseOrderLineLookup lines, IProcessedEventStore processedEvents)
    {
        _uow = uow;
        _lines = lines;
        _processedEvents = processedEvents;
    }

    public async Task Handle(PurchaseOrderReceivedNotification notification, CancellationToken ct)
    {
        // Guard domain state: a PO whose receipts already exist is never re-projected.
        if (await _uow.QcReceipts.ExistsForPurchaseOrderAsync(notification.PurchaseOrderId, ct))
            return;

        // Consumer-owned idempotency: the marker and the receipts commit in one transaction,
        // so a retry after a partial failure can never double-produce receipts.
        if (await _processedEvents.HasProcessedAsync(notification.EventId, nameof(PurchaseOrderReceivedHandler), ct))
            return;

        foreach (var lineId in notification.LineItemIds)
        {
            var line = await _lines.GetAsync(lineId, ct)
                ?? throw new InvalidOperationException($"Purchase order line {lineId} was not found.");

            var receipt = QcReceipt.Create(
                notification.PurchaseOrderId, line.ItemId, line.ProductTypeId,
                line.BatchNo, line.Quantity, line.UomId);
            await _uow.QcReceipts.AddAsync(receipt, ct);
        }

        await _processedEvents.MarkProcessedAsync(notification.EventId, nameof(PurchaseOrderReceivedHandler), ct);
        await _uow.SaveChangesAsync(ct);
    }
}
