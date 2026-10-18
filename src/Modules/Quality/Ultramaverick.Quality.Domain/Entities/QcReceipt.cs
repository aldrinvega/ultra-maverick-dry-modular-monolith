using Ultramaverick.Quality.Contracts;
using Ultramaverick.Quality.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

public sealed class QcReceipt : Entity
{
    private readonly List<QcReject> _rejects = new();
    private readonly List<Checklist> _checklists = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int PurchaseOrderId { get; private set; }        // scalar → Procurement
    public int? PurchaseOrderLineId { get; private set; }
    public int ItemId { get; private set; }                 // scalar → Catalog.Items
    public int UomId { get; private set; }                  // scalar → Catalog.Uoms
    public int? ProductTypeId { get; private set; }
    public string? BatchNo { get; private set; }
    public DateTime? ManufacturingDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public decimal ActualDelivered { get; private set; }
    public decimal TotalReject { get; private set; }
    public bool IsNearlyExpire { get; private set; }
    public int? ExpiryApprovedByUserId { get; private set; }
    public DateTime? ExpiryApprovedAtUtc { get; private set; }
    public int? QcByUserId { get; private set; }
    public int? MonitoredByUserId { get; private set; }
    public bool IsWarehouseReceived { get; private set; }
    public string Status { get; private set; } = "ForReceiving";
    public int? CancelReasonId { get; private set; }
    public string? CancelRemarks { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<QcReject> Rejects => _rejects.AsReadOnly();
    public IReadOnlyCollection<Checklist> Checklists => _checklists.AsReadOnly();

    private QcReceipt() { }

    public static QcReceipt Create(
        int purchaseOrderId, int itemId, int? productTypeId, string? batchNo, decimal actualDelivered, int uomId)
    {
        if (purchaseOrderId <= 0) throw new ArgumentOutOfRangeException(nameof(purchaseOrderId));
        if (itemId <= 0) throw new ArgumentOutOfRangeException(nameof(itemId));
        if (uomId <= 0) throw new ArgumentOutOfRangeException(nameof(uomId));

        var r = new QcReceipt
        {
            PurchaseOrderId = purchaseOrderId, ItemId = itemId, UomId = uomId, ProductTypeId = productTypeId,
            BatchNo = batchNo, ActualDelivered = actualDelivered, Status = "ForReceiving", IsActive = true
        };
        r.RaiseDomainEvent(new QcReceiptChanged("Created", DateTime.UtcNow));
        return r;
    }

    public void Reject(decimal quantity, int? reasonId, string? remarks, int? rejectedByUserId)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (quantity > ActualDelivered - TotalReject)
            throw new InvalidOperationException("Reject quantity exceeds remaining delivered quantity.");

        _rejects.Add(QcReject.Create(Id, quantity, reasonId, remarks, rejectedByUserId));
        TotalReject += quantity;
        RaiseDomainEvent(new QcReceiptChanged("Rejected", DateTime.UtcNow));
    }

    public void ApproveExpiry(int approvedByUserId)
    {
        if (!IsNearlyExpire) return;
        IsNearlyExpire = false;
        ExpiryApprovedByUserId = approvedByUserId;
        ExpiryApprovedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new QcReceiptChanged("ExpiryApproved", DateTime.UtcNow));
    }

    public void Complete(int qcByUserId)
    {
        if (Status == "Completed") return;
        Status = "Completed";
        QcByUserId = qcByUserId;
        RaiseDomainEvent(new QcReceiptCompleted(
            Id, ItemId, UomId, BatchNo, ActualDelivered - TotalReject,
            ManufacturingDate, ExpiryDate, QcByUserId, DateTime.UtcNow));
    }
}
