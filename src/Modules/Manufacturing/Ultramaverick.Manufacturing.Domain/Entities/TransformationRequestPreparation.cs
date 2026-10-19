using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Domain.Entities;

/// <summary>A preparation (weighing) record for a transformation request.</summary>
public sealed class TransformationRequestPreparation : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int TransformationRequestId { get; private set; } // scalar; populated by EF on save
    public int? TransformationRequestLineId { get; private set; }
    public int? PreparedByUserId { get; private set; }
    public DateTime PreparedAtUtc { get; private set; }
    public decimal QuantityNeeded { get; private set; }
    public string? WeighingScale { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private TransformationRequestPreparation() { }

    public static TransformationRequestPreparation Create(
        int? transformationRequestLineId, int preparedByUserId, decimal quantityNeeded)
    {
        if (preparedByUserId <= 0) throw new ArgumentOutOfRangeException(nameof(preparedByUserId));
        if (quantityNeeded < 0) throw new ArgumentOutOfRangeException(nameof(quantityNeeded));

        return new TransformationRequestPreparation
        {
            TransformationRequestLineId = transformationRequestLineId,
            PreparedByUserId = preparedByUserId,
            PreparedAtUtc = DateTime.UtcNow,
            QuantityNeeded = quantityNeeded
        };
    }
}
