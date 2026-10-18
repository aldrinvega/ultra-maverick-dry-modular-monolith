using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Application.Models;

public sealed record WarehouseReceiptDto(
    int Id,
    int? QcReceiptId,
    int WarehouseId,
    int ItemId,
    int UomId,
    string? LotNo,
    decimal Quantity,
    DateTime? ManufacturingDate,
    DateTime? ExpiryDate,
    DateTime ReceivedAtUtc,
    bool IsActive)
{
    public static WarehouseReceiptDto From(WarehouseReceipt r) => new(
        r.Id, r.QcReceiptId, r.WarehouseId, r.ItemId, r.UomId, r.LotNo,
        r.Quantity, r.ManufacturingDate, r.ExpiryDate, r.ReceivedAtUtc, r.IsActive);
}
