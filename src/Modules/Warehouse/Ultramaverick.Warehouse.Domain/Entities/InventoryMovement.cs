using Ultramaverick.SharedKernel;

namespace Ultramaverick.Warehouse.Domain.Entities;

/// <summary>
/// Append-only ledger row. Standalone root (not a child of a balance) so it survives any
/// balance rebuild; on-hand is always derivable from the movements written here.
/// </summary>
public sealed class InventoryMovement : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int WarehouseId { get; private set; }
    public int ItemId { get; private set; }
    public int UomId { get; private set; }
    public string? LotNo { get; private set; }
    public decimal Delta { get; private set; }
    public MovementSource Source { get; private set; }
    public int? SourceDocumentId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }

    private InventoryMovement() { }

    public static InventoryMovement Create(
        int warehouseId, int itemId, int uomId, string? lotNo, decimal delta,
        MovementSource source, int? sourceDocumentId)
        => new()
        {
            WarehouseId = warehouseId,
            ItemId = itemId,
            UomId = uomId,
            LotNo = lotNo,
            Delta = delta,
            Source = source,
            SourceDocumentId = sourceDocumentId
        };
}
