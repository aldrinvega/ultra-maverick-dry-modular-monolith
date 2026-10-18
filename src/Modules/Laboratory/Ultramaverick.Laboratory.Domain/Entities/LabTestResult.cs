using Ultramaverick.Laboratory.Contracts;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Laboratory.Domain.Entities;

public sealed class LabTestResult : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int LabReceiveRequestId { get; private set; }   // real FK -> LabReceiveRequests
    public DateTime? DateAnalyzed { get; private set; }
    public string? Result { get; private set; }
    public string Status { get; private set; } = LaboratoryStatuses.ForQaApproval;
    public string? Remarks { get; private set; }
    public int? AnalyzedByUserId { get; private set; }
    public bool IsActive { get; private set; }

    private LabTestResult() { }

    public static LabTestResult Record(
        int labReceiveRequestId, int labTestRequestId, int? warehouseReceiptId, string? batchNo,
        DateTime analyzedAt, string result, int analyzedByUserId)
    {
        var entity = new LabTestResult
        {
            LabReceiveRequestId = labReceiveRequestId,
            DateAnalyzed = analyzedAt,
            Result = result,
            Status = LaboratoryStatuses.ForQaApproval,
            AnalyzedByUserId = analyzedByUserId,
            IsActive = true
        };

        entity.RaiseDomainEvent(new LabResultRecorded(
            labReceiveRequestId, labTestRequestId, warehouseReceiptId, batchNo,
            result, entity.Status, analyzedByUserId, analyzedAt, DateTime.UtcNow));

        return entity;
    }

    public void Approve(int approvedByUserId)
    {
        if (Status != LaboratoryStatuses.ForQaApproval)
            throw new ArgumentOutOfRangeException(nameof(Status));

        Status = LaboratoryStatuses.Approved;
        RaiseDomainEvent(new LabResultApproved(
            LabReceiveRequestId, Status, approvedByUserId, DateTime.UtcNow));
    }
}
