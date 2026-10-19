using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Contracts;

/// <summary>
/// Raised when prepared materials are finished. Warehouse consumes this to create the
/// receipt; the payload carries everything that consumer needs so it never looks back
/// into the Manufacturing schema.
/// </summary>
public sealed record TransformationCompleted(
    string ChangeType,
    int ProductionPlanId,
    int WarehouseId,
    DateTime OccurredAtUtc) : IDomainEvent;
