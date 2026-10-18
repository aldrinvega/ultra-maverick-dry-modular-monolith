using Ultramaverick.SharedKernel;

namespace Ultramaverick.Warehouse.Domain.Entities;

public sealed class InventoryBalance : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int WarehouseId { get; private set; }
    public int ItemId { get; private set; }
    public int UomId { get; private set; }
    public string? LotNo { get; private set; }
    public decimal OnHand { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;

    private InventoryBalance() { }

    public static InventoryBalance Create(int warehouseId, int itemId, int uomId, string? lotNo)
        => new() { WarehouseId = warehouseId, ItemId = itemId, UomId = uomId, LotNo = lotNo };

    public void Apply(decimal delta)
    {
        var next = OnHand + delta;
        if (next < 0)
            throw new InvalidOperationException(
                $"Insufficient stock for item {ItemId} (on hand {OnHand}, requested {delta}).");

        OnHand = next;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
