using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Contracts;

/// <summary>Raised when a requested transformation request is approved.</summary>
public sealed record TransformationApproved(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;
