using Ultramaverick.SharedKernel;

namespace Ultramaverick.Warehouse.Domain.Entities;

public sealed class MiscellaneousReceiptLine : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int MiscellaneousReceiptId { get; private set; }
    public int ItemId { get; private set; }
    public int UomId { get; private set; }
    public string? LotNo { get; private set; }
    public decimal Quantity { get; private set; }
    public string? Remarks { get; private set; }

    private MiscellaneousReceiptLine() { }
}
