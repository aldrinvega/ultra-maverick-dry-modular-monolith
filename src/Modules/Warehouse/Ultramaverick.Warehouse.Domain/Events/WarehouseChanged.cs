using Ultramaverick.SharedKernel;

namespace Ultramaverick.Warehouse.Domain.Events;

public static class WarehouseChangeTypes
{
    public const string Created = nameof(Created);
    public const string Updated = nameof(Updated);
    public const string Activated = nameof(Activated);
    public const string Deactivated = nameof(Deactivated);
}

// Id-free on purpose: at raise time the entity has no identity yet, so an id in the
// payload would always be 0. The outbox row carries the assigned id in AggregateId.
public sealed record WarehouseChanged(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;
