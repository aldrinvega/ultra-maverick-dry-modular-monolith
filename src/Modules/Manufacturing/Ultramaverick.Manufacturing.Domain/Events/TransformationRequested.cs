using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Domain.Events;

/// <summary>Raised when a transformation request is created. Id-free on purpose: the request id
/// is preserved as the outbox AggregateId.</summary>
public sealed record TransformationRequested(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;
