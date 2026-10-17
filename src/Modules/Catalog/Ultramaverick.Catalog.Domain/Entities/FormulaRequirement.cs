using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class FormulaRequirement : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int FormulaHeaderId { get; private set; }
    public int RawMaterialId { get; private set; }
    public decimal Quantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private FormulaRequirement() { }

    internal static FormulaRequirement Create(int headerId, int rawMaterialId, decimal quantity)
    {
        if (rawMaterialId <= 0) throw new ArgumentOutOfRangeException(nameof(rawMaterialId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        return new FormulaRequirement
        {
            FormulaHeaderId = headerId,
            RawMaterialId = rawMaterialId,
            Quantity = quantity,
            IsActive = true
        };
    }
}