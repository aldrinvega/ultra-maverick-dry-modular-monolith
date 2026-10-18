using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Domain.Entities;

/// <summary>A rejection snapshot taken when a transformation request is rejected.</summary>
public sealed class TransformationRequestReject : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int TransformationRequestId { get; private set; } // scalar; populated by EF on save
    public int ReasonId { get; private set; }                // scalar → Catalog (no FK)
    public decimal Quantity { get; private set; }
    public DateTime RejectedAtUtc { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private TransformationRequestReject() { }

    public static TransformationRequestReject Create(int reasonId, decimal quantity)
    {
        if (reasonId <= 0) throw new ArgumentOutOfRangeException(nameof(reasonId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        return new TransformationRequestReject
        {
            ReasonId = reasonId,
            Quantity = quantity,
            RejectedAtUtc = DateTime.UtcNow
        };
    }
}
