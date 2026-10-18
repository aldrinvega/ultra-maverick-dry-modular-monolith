using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Domain.Entities;

/// <summary>A required raw material of a transformation request.</summary>
public sealed class TransformationRequestLine : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int TransformationRequestId { get; private set; } // scalar; populated by EF on save
    public int ItemId { get; private set; }                  // scalar → Catalog (no FK)
    public decimal RequiredQuantity { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private TransformationRequestLine() { }

    public static TransformationRequestLine Create(int itemId, decimal requiredQuantity)
    {
        if (itemId <= 0) throw new ArgumentOutOfRangeException(nameof(itemId));
        if (requiredQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(requiredQuantity));

        return new TransformationRequestLine
        {
            ItemId = itemId,
            RequiredQuantity = requiredQuantity
        };
    }
}
