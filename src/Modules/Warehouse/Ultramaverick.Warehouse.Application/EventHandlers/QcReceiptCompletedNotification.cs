using MediatR;

namespace Ultramaverick.Warehouse.Application.EventHandlers;

/// <summary>
/// In-process notification the Quality bridge publishes when a QC receipt completes.
/// <paramref name="EventId"/> is carried on the notification so the consumer-owned
/// processed-events guard can be applied atomically with the receipt it writes.
/// </summary>
public sealed record QcReceiptCompletedNotification(
    int QcReceiptId,
    int ItemId,
    int UomId,
    string? LotNo,
    decimal NetQuantity,
    DateTime? ManufacturingDate,
    DateTime? ExpiryDate,
    int? CompletedBy,
    Guid EventId) : INotification;
