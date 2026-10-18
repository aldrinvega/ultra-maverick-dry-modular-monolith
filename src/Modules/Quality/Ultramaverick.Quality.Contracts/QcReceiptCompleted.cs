using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Contracts;

// Cross-module integration contract: published by Quality, consumed by Warehouse (§9.2/§13.3).
// POCO only — no EF, no domain. This is the only legal Quality -> other-module surface.
// The field list mirrors WarehouseReceipt.Create so Warehouse (Wave 6) needs no rework.
public sealed record QcReceiptCompleted(
    int QcReceiptId,
    int ItemId,
    int UomId,
    string? LotNo,
    decimal NetQuantity,
    DateTime? ManufacturingDate,
    DateTime? ExpiryDate,
    int? CompletedBy,
    DateTime OccurredAtUtc) : IDomainEvent;
