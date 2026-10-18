using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

public sealed class QcReject : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int QcReceiptId { get; private set; }
    public decimal Quantity { get; private set; }
    public int? ReasonId { get; private set; }
    public string? Remarks { get; private set; }
    public int? RejectedByUserId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private QcReject() { }

    internal static QcReject Create(
        int qcReceiptId, decimal quantity, int? reasonId, string? remarks, int? rejectedByUserId)
        => new()
        {
            QcReceiptId = qcReceiptId,
            Quantity = quantity,
            ReasonId = reasonId,
            Remarks = remarks,
            RejectedByUserId = rejectedByUserId,
            IsActive = true
        };
}
