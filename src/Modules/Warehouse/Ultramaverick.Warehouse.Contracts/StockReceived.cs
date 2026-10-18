using Ultramaverick.SharedKernel;

namespace Ultramaverick.Warehouse.Contracts;

// Cross-module integration contract: published by Warehouse, consumed by Reporting (§9.2/§12.2).
// POCO only — no EF, no domain. This is the only legal Warehouse -> other-module surface.
// Id-free on purpose: WarehouseReceipt.Create is a factory on a new entity, so an id in the payload
// would always be 0 at raise time. The receipt id is preserved as the outbox AggregateId.
public sealed record StockReceived(
    int WarehouseId,
    int ItemId,
    int UomId,
    string? LotNo,
    decimal Quantity,
    DateTime OccurredAtUtc) : IDomainEvent;
