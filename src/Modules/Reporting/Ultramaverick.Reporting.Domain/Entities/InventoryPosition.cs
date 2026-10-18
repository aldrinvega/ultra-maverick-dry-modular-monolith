namespace Ultramaverick.Reporting.Domain.Entities;

/// <summary>On-hand stock for one item in one warehouse lot, keyed by (warehouse, item, lot).</summary>
public sealed class InventoryPosition
{
    public int WarehouseId { get; private set; }
    public int ItemId { get; private set; }
    public string LotNo { get; private set; } = null!;
    public int UomId { get; private set; }
    public decimal Quantity { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private InventoryPosition() { }

    public static InventoryPosition Create(
        int warehouseId, int itemId, int uomId, string? lotNo, DateTime occurredAtUtc)
        => new()
        {
            WarehouseId = warehouseId,
            ItemId = itemId,
            UomId = uomId,
            LotNo = NormalizeLot(lotNo),
            Quantity = 0m,
            UpdatedAtUtc = occurredAtUtc
        };

    public void AddOnHand(decimal quantity, DateTime occurredAtUtc)
    {
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        Quantity += quantity;
        UpdatedAtUtc = occurredAtUtc;
    }

    /// <summary>The key column cannot be null, so an absent lot is stored as the empty string.</summary>
    public static string NormalizeLot(string? lotNo)
        => string.IsNullOrWhiteSpace(lotNo) ? string.Empty : lotNo;
}
