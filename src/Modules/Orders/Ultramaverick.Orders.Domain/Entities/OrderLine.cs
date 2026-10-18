using Ultramaverick.SharedKernel;

namespace Ultramaverick.Orders.Domain.Entities;

public sealed class OrderLine : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int OrderId { get; private set; }               // scalar; populated by EF on save
    public int ItemId { get; private set; }                // scalar → Catalog (no FK)
    public int UomId { get; private set; }                 // scalar → Catalog (no FK)
    public decimal OriginalQuantityOrdered { get; private set; }
    public decimal QuantityOrdered { get; private set; }
    public decimal AllocatedQuantity { get; private set; }
    public bool ForAllocation { get; private set; }
    public bool IsBeingPrepared { get; private set; }
    public int? WarehouseId { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public string? PlateNumber { get; private set; }
    public string? DeliveryStatus { get; private set; }
    public DateTime? ReceivedDate { get; private set; }
    public int? FormulaHeaderId { get; private set; }      // scalar → Catalog (no FK)
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private OrderLine() { }

    public static OrderLine Create(int itemId, int uomId, decimal quantity, int? formulaHeaderId)
    {
        if (itemId <= 0) throw new ArgumentOutOfRangeException(nameof(itemId));
        if (uomId <= 0) throw new ArgumentOutOfRangeException(nameof(uomId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        return new OrderLine
        {
            ItemId = itemId,
            UomId = uomId,
            OriginalQuantityOrdered = quantity,
            QuantityOrdered = quantity,
            FormulaHeaderId = formulaHeaderId
        };
    }
}
