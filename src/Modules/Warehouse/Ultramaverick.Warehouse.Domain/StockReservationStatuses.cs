namespace Ultramaverick.Warehouse.Domain;

/// <summary>The status values carried by a stock reservation. Kept as constants so the
/// magic strings live in one place, mirroring the Laboratory status convention.</summary>
public static class StockReservationStatuses
{
    public const string Held = nameof(Held);
    public const string Released = nameof(Released);
    public const string Consumed = nameof(Consumed);
    public const string Expired = nameof(Expired);
}
