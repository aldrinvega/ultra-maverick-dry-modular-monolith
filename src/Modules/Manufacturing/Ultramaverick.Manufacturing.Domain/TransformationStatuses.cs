namespace Ultramaverick.Manufacturing.Domain;

/// <summary>The status values carried by a transformation request. Kept as constants so the
/// magic strings live in one place, mirroring the Orders/Warehouse conventions.</summary>
public static class TransformationStatuses
{
    public const string Requested = nameof(Requested);
    public const string Prepared = nameof(Prepared);
    public const string Cancelled = nameof(Cancelled);
}
