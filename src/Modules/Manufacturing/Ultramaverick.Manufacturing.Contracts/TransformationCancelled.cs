using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Contracts;

/// <summary>Raised when a transformation request is cancelled before preparation.</summary>
public sealed record TransformationCancelled(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;
