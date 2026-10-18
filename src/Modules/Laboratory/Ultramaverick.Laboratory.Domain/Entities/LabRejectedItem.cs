using Ultramaverick.SharedKernel;

namespace Ultramaverick.Laboratory.Domain.Entities;

public sealed class LabRejectedItem : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int LabReceiveRequestId { get; private set; }
    public decimal? Quantity { get; private set; }
    public int? ReasonId { get; private set; }
    public string? Remarks { get; private set; }
    public int? RejectedByUserId { get; private set; }
    public DateTime? RejectedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private LabRejectedItem() { }

    internal static LabRejectedItem Create(
        int labReceiveRequestId, decimal? quantity, int? reasonId, string? remarks,
        int? rejectedByUserId, DateTime? rejectedAtUtc)
        => new()
        {
            LabReceiveRequestId = labReceiveRequestId,
            Quantity = quantity,
            ReasonId = reasonId,
            Remarks = remarks,
            RejectedByUserId = rejectedByUserId,
            RejectedAtUtc = rejectedAtUtc,
            IsActive = true
        };
}
