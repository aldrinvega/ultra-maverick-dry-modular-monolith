using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Domain.Events;

/// <summary>Raised when a transformation request is rejected. This is deliberately a distinct
/// event from <see cref="TransformationRequested"/>; the guide's aggregate re-raised the request
/// event here by mistake.</summary>
public sealed record TransformationRejected(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;
