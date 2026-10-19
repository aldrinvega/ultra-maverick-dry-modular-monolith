using Ultramaverick.Manufacturing.Contracts;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Domain.Entities;

/// <summary>
/// A request to transform raw materials into a planned item. The guide's aggregate is followed
/// verbatim except for the two defects the wave rulings corrected: events never carry an id, and
/// rejection raises its own event.
/// </summary>
public sealed class TransformationRequest : Entity
{
    private readonly List<TransformationRequestLine> _lines = new();
    private readonly List<TransformationRequestPreparation> _preparations = new();
    private readonly List<TransformationRequestReject> _rejects = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ProductionPlanId { get; private set; }  // FK â†’ Manufacturing.ProductionPlans
    public int WarehouseId { get; private set; }       // scalar â†’ Warehouse (no FK)
    public string Status { get; private set; } = TransformationStatuses.Requested;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    public IReadOnlyCollection<TransformationRequestLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<TransformationRequestPreparation> Preparations => _preparations.AsReadOnly();
    public IReadOnlyCollection<TransformationRequestReject> Rejects => _rejects.AsReadOnly();

    private TransformationRequest() { }

    public static TransformationRequest Create(
        int productionPlanId,
        int warehouseId,
        int? createdByUserId,
        IEnumerable<TransformationLineInput> lines)
    {
        if (productionPlanId <= 0) throw new ArgumentOutOfRangeException(nameof(productionPlanId));
        if (warehouseId <= 0) throw new ArgumentOutOfRangeException(nameof(warehouseId));
        ArgumentNullException.ThrowIfNull(lines);

        var request = new TransformationRequest
        {
            ProductionPlanId = productionPlanId,
            WarehouseId = warehouseId,
            Status = TransformationStatuses.Requested,
            CreatedByUserId = createdByUserId
        };

        // The children take no parent id: EF wires the FK from the nav collection on save.
        foreach (var line in lines)
            request._lines.Add(TransformationRequestLine.Create(line.ItemId, line.RequiredQuantity));

        if (request._lines.Count == 0)
            throw new ArgumentException("A transformation request must have at least one line.", nameof(lines));

        request.RaiseDomainEvent(
            new TransformationRequested(TransformationChangeTypes.Created, DateTime.UtcNow));

        return request;
    }

    public void Prepare(int preparedByUserId)
    {
        var occurredAt = DateTime.UtcNow;
        var quantityNeeded = _lines.Sum(line => line.RequiredQuantity);

        Status = TransformationStatuses.Prepared;
        ModifiedAtUtc = occurredAt;
        ModifiedByUserId = preparedByUserId;

        _preparations.Add(TransformationRequestPreparation.Create(preparedByUserId, quantityNeeded));

        RaiseDomainEvent(new TransformationPrepared(TransformationChangeTypes.Prepared, occurredAt));
    }

    public void Reject(int rejectReasonId, decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        if (Status != TransformationStatuses.Requested)
            throw new InvalidOperationException("Only a requested transformation request can be rejected.");

        var occurredAt = DateTime.UtcNow;
        ModifiedAtUtc = occurredAt;

        _rejects.Add(TransformationRequestReject.Create(rejectReasonId, quantity));

        RaiseDomainEvent(new TransformationRejected(TransformationChangeTypes.Rejected, occurredAt));
    }

    public void Cancel(int? cancelledByUserId)

    {
        if (Status == TransformationStatuses.Cancelled)
            return;

        var occurredAt = DateTime.UtcNow;
        Status = TransformationStatuses.Cancelled;
        ModifiedAtUtc = occurredAt;
        ModifiedByUserId = cancelledByUserId;

        RaiseDomainEvent(new TransformationCancelled(TransformationChangeTypes.Cancelled, occurredAt));
    }

    public void Approve(int? approvedByUserId)
    {
        if (Status != TransformationStatuses.Requested)
            throw new InvalidOperationException("Only a requested transformation request can be approved.");

        var occurredAt = DateTime.UtcNow;
        Status = TransformationStatuses.Approved;
        ModifiedAtUtc = occurredAt;
        ModifiedByUserId = approvedByUserId;

        RaiseDomainEvent(new TransformationApproved(TransformationChangeTypes.Approved, occurredAt));
    }
}
