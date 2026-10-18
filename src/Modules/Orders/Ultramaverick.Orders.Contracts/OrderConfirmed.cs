using Ultramaverick.SharedKernel;

namespace Ultramaverick.Orders.Contracts;

// Id-free on purpose: raised by Order.Confirm, which carries the business key OrderNumber.
public sealed record OrderConfirmed(string OrderNumber, DateTime OccurredAtUtc) : IDomainEvent;
