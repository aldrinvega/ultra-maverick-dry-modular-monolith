using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Application.Models;

public sealed record InventoryBalanceDto(
    int Id,
    int WarehouseId,
    int ItemId,
    int UomId,
    string? LotNo,
    decimal OnHand,
    DateTime UpdatedAtUtc)
{
    public static InventoryBalanceDto From(InventoryBalance b) => new(
        b.Id, b.WarehouseId, b.ItemId, b.UomId, b.LotNo, b.OnHand, b.UpdatedAtUtc);
}
