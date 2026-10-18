using Ultramaverick.SharedKernel;

namespace Ultramaverick.Procurement.Contracts;

// Cross-module integration contract: published by Procurement, consumed by Quality (§7.4/§13.3).
// POCO only — no EF, no domain. This is the only legal Procurement -> other-module surface.
public sealed record PurchaseOrderReceived(
    int PurchaseOrderId,
    int SupplierId,
    IReadOnlyList<int> LineItemIds,
    DateTime OccurredAtUtc) : IDomainEvent;
