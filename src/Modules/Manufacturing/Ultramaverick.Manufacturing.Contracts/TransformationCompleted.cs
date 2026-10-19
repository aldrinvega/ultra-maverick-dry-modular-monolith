using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Contracts;

/// <summary>One completed line: what to receive into stock. Carries no unit of measure because
/// Manufacturing does not store one; the Warehouse consumer resolves it from Catalog.</summary>
public sealed record TransformationCompletedLine(int ItemId, decimal Quantity);

/// <summary>
/// Raised when prepared materials are finished. Warehouse consumes this to create receipts;
/// the payload carries everything that consumer needs so it never reads the Manufacturing schema.
/// </summary>
public sealed record TransformationCompleted(
    string ChangeType,
    int ProductionPlanId,
    int WarehouseId,
    IReadOnlyList<TransformationCompletedLine> Lines,
    DateTime OccurredAtUtc) : IDomainEvent;
