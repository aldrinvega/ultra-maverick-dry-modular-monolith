using Ultramaverick.SharedKernel;

namespace Ultramaverick.Warehouse.Domain.Entities;

/// <summary>Map-only placeholder for a held allocation. No behaviour this wave (G42).</summary>
public sealed class StockReservation : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int OrderId { get; private set; }               // scalar → Orders (no FK)
    public int WarehouseId { get; private set; }
    public int ItemId { get; private set; }
    public int UomId { get; private set; }
    public string? LotNo { get; private set; }
    public decimal Quantity { get; private set; }
    public string Status { get; private set; } = StockReservationStatuses.Held;
    public DateTime ReservedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; private set; }
    public int? ReservedByUserId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private StockReservation() { }
}
