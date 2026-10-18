using Ultramaverick.SharedKernel;

namespace Ultramaverick.Laboratory.Domain.Entities;

public sealed class LabReceiveRequest : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int LabTestRequestId { get; private set; }
    public int? DispositionId { get; private set; }
    public DateTime? DateReceived { get; private set; }
    public int? ReceivedByUserId { get; private set; }
    public string Status { get; private set; } = LaboratoryStatuses.Accepted;
    public string? Remarks { get; private set; }
    public bool IsActive { get; private set; }

    private LabReceiveRequest() { }

    public static LabReceiveRequest Create(
        int labTestRequestId, int? dispositionId, DateTime? dateReceived,
        int? receivedByUserId, string? remarks)
        => new()
        {
            LabTestRequestId = labTestRequestId,
            DispositionId = dispositionId,
            DateReceived = dateReceived,
            ReceivedByUserId = receivedByUserId,
            Status = LaboratoryStatuses.Accepted,
            Remarks = remarks,
            IsActive = true
        };
}
