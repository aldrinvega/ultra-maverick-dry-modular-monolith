using Ultramaverick.SharedKernel;

namespace Ultramaverick.Procurement.Domain.Events;

public sealed record PurchaseOrderChanged(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;
