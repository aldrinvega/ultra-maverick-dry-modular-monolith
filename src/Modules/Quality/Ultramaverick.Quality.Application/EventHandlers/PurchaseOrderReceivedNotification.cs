using MediatR;

namespace Ultramaverick.Quality.Application.EventHandlers;

/// <summary>
/// In-process notification the Procurement bridge publishes when a purchase order is received.
/// <paramref name="EventId"/> is carried on the notification so the consumer-owned
/// processed-events guard can be applied atomically with the receipts it writes (§13.1 note).
/// </summary>
public sealed record PurchaseOrderReceivedNotification(
    int PurchaseOrderId,
    int SupplierId,
    IReadOnlyList<int> LineItemIds,
    Guid EventId) : INotification;
