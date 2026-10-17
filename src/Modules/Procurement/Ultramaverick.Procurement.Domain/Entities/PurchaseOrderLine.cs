using Ultramaverick.SharedKernel;

namespace Ultramaverick.Procurement.Domain.Entities;

public sealed class PurchaseOrderLine : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int PurchaseOrderId { get; private set; }
    public int ItemId { get; private set; }        // scalar → Catalog.Items
    public int UomId { get; private set; }         // scalar → Catalog.Uoms
    public decimal OrderedQuantity { get; private set; }
    public decimal DeliveredQuantity { get; private set; }
    public decimal BilledQuantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public bool IsActive { get; private set; }

    private PurchaseOrderLine() { }

    internal static PurchaseOrderLine Create(
        int headerId, int itemId, int uomId, decimal orderedQuantity, decimal unitPrice)
        => new()
        {
            PurchaseOrderId = headerId, ItemId = itemId, UomId = uomId,
            OrderedQuantity = orderedQuantity, UnitPrice = unitPrice, IsActive = true
        };

    internal void AddDelivery(decimal quantity) => DeliveredQuantity += quantity;
}
