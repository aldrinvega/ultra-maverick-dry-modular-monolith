using Ultramaverick.SharedKernel;

namespace Ultramaverick.Laboratory.Domain.Events;

public static class LabTestRequestChangeTypes
{
    public const string Created = nameof(Created);
}

// Id-free on purpose: at raise time the entity has no identity yet, so an id in the
// payload would always be 0. The outbox row carries the assigned id in AggregateId.
public sealed record LabTestRequestChanged(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;
