using Ultramaverick.SharedKernel;

namespace Ultramaverick.Orders.Contracts;

// Cross-module integration contract: published by Orders, consumed by Reporting (§9.2/§12.2).
// POCO only — no EF, no domain. This is the only legal Orders -> other-module surface.
// Id-free on purpose: Order.Create is a factory on a new entity, so an id in the payload would
// always be 0 at raise time. The business key (OrderNumber) is carried instead, and the order id
// is preserved as the outbox AggregateId.
public sealed record OrderCreated(string OrderNumber, int CustomerId, DateTime OccurredAtUtc) : IDomainEvent;
