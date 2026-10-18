namespace Ultramaverick.Reporting.Domain.Entities;

/// <summary>The latest known quality status of one QC receipt.</summary>
public sealed class QcStatus
{
    public int QcReceiptId { get; private set; }
    public int ItemId { get; private set; }
    public int UomId { get; private set; }
    public string? LotNo { get; private set; }
    public decimal NetQuantity { get; private set; }
    public string Status { get; private set; } = ReportingStatuses.Completed;
    public DateTime OccurredAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private QcStatus() { }

    public static QcStatus Create(
        int qcReceiptId, int itemId, int uomId, string? lotNo, decimal netQuantity, DateTime occurredAtUtc)
        => new()
        {
            QcReceiptId = qcReceiptId,
            ItemId = itemId,
            UomId = uomId,
            LotNo = lotNo,
            NetQuantity = netQuantity,
            Status = ReportingStatuses.Completed,
            OccurredAtUtc = occurredAtUtc,
            UpdatedAtUtc = occurredAtUtc
        };

    public void Update(decimal netQuantity, DateTime occurredAtUtc)
    {
        NetQuantity = netQuantity;
        Status = ReportingStatuses.Completed;
        UpdatedAtUtc = occurredAtUtc;
    }
}
