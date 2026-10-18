using Ultramaverick.SharedKernel;
using Ultramaverick.Warehouse.Contracts;

namespace Ultramaverick.Warehouse.Domain.Entities;

public sealed class WarehouseReceipt : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int? QcReceiptId { get; private set; }        // scalar → Quality (UQ when present)
    public int WarehouseId { get; private set; }
    public int ItemId { get; private set; }
    public int UomId { get; private set; }
    public string? LotNo { get; private set; }
    public decimal Quantity { get; private set; }
    public DateTime? ManufacturingDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? ReceivedByUserId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private WarehouseReceipt() { }

    public static WarehouseReceipt Create(
        int? qcReceiptId, int warehouseId, int itemId, int uomId,
        string? lotNo, decimal quantity, DateTime? manufacturingDate, DateTime? expiryDate, int? receivedBy)
    {
        if (warehouseId <= 0) throw new ArgumentOutOfRangeException(nameof(warehouseId));
        if (itemId <= 0) throw new ArgumentOutOfRangeException(nameof(itemId));
        if (uomId <= 0) throw new ArgumentOutOfRangeException(nameof(uomId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        var receipt = new WarehouseReceipt
        {
            QcReceiptId = qcReceiptId,
            WarehouseId = warehouseId,
            ItemId = itemId,
            UomId = uomId,
            LotNo = lotNo,
            Quantity = quantity,
            ManufacturingDate = manufacturingDate,
            ExpiryDate = expiryDate,
            ReceivedByUserId = receivedBy
        };

        receipt.RaiseDomainEvent(new StockReceived(
            warehouseId, itemId, uomId, lotNo, quantity, DateTime.UtcNow));

        return receipt;
    }
}
