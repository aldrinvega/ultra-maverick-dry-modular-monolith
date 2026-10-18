using Ultramaverick.Procurement.Contracts;
using Ultramaverick.Procurement.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Procurement.Domain.Entities;

public sealed class PurchaseOrder : Entity
{
    private readonly List<PurchaseOrderLine> _lines = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string PoNumber { get; private set; } = null!;
    public string? PrNumber { get; private set; }
    public DateTime? PoDate { get; private set; }
    public DateTime? PrDate { get; private set; }
    public int SupplierId { get; private set; }        // scalar → Catalog.Suppliers
    public int? ImportBatchId { get; private set; }
    public string Status { get; private set; } = "Open";
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    private PurchaseOrder() { }

    public static PurchaseOrder Create(
        string poNumber, string? prNumber, DateTime? poDate, DateTime? prDate,
        int supplierId, int? importBatchId, int? createdByUserId)
    {
        if (supplierId <= 0) throw new ArgumentOutOfRangeException(nameof(supplierId));
        poNumber = (poNumber ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(poNumber);

        var po = new PurchaseOrder
        {
            PoNumber = poNumber, PrNumber = prNumber, PoDate = poDate, PrDate = prDate,
            SupplierId = supplierId, ImportBatchId = importBatchId,
            Status = "Open", IsActive = true, CreatedByUserId = createdByUserId
        };
        po.RaiseDomainEvent(new PurchaseOrderChanged("Created", DateTime.UtcNow));
        return po;
    }

    public PurchaseOrderLine AddLine(int itemId, int uomId, decimal orderedQuantity, decimal unitPrice)
    {
        if (orderedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(orderedQuantity));

        var line = PurchaseOrderLine.Create(Id, itemId, uomId, orderedQuantity, unitPrice);
        _lines.Add(line);
        RaiseDomainEvent(new PurchaseOrderChanged("LineAdded", DateTime.UtcNow));
        return line;
    }

    public void MarkReceived(int modifiedByUserId)
    {
        if (Status == "Received") return;
        Status = "Received";
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new PurchaseOrderReceived(
            Id, SupplierId, _lines.Select(l => l.Id).ToList(), DateTime.UtcNow));
    }
}
