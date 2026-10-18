using Ultramaverick.SharedKernel;

namespace Ultramaverick.Orders.Contracts;

// Id-free on purpose: raised by Order.Cancel, which carries the business key OrderNumber.
public sealed record OrderCancelled(string OrderNumber, string? Reason, DateTime OccurredAtUtc) : IDomainEvent;
