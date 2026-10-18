using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Domain.Events;

/// <summary>Raised when the materials of a transformation request have been prepared.</summary>
public sealed record TransformationPrepared(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;
