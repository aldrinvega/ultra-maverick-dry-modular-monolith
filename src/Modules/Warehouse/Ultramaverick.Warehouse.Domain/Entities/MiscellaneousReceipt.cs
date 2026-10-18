using Ultramaverick.SharedKernel;

namespace Ultramaverick.Warehouse.Domain.Entities;

/// <summary>Map-only placeholder for a non-PO goods receipt (manual intake). No behaviour this wave.</summary>
public sealed class MiscellaneousReceipt : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int WarehouseId { get; private set; }
    public string? RefNo { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; } = DateTime.UtcNow;
    public string? Remarks { get; private set; }
    public int? ReceivedByUserId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<MiscellaneousReceiptLine> Lines { get; private set; } = new List<MiscellaneousReceiptLine>();

    private MiscellaneousReceipt() { }
}
