using Ultramaverick.SharedKernel;

namespace Ultramaverick.Procurement.Domain.Events;

// Published to the outbox for Quality to consume.
public sealed record PurchaseOrderReceived(int PurchaseOrderId, int SupplierId, DateTime OccurredAtUtc)
    : IDomainEvent;
