namespace Ultramaverick.Orders.Domain;

/// <summary>The status values carried by an order. Kept as constants so the magic strings
/// live in one place, mirroring the Warehouse status convention.</summary>
public static class OrderStatuses
{
    public const string Draft = nameof(Draft);
    public const string Confirmed = nameof(Confirmed);
    public const string Cancelled = nameof(Cancelled);
}
