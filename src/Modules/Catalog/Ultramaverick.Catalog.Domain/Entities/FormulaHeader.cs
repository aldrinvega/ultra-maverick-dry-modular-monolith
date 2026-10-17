using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class FormulaHeader : Entity
{
    private readonly List<FormulaRequirement> _requirements = [];

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ItemId { get; private set; }
    public int Version { get; private set; }
    public decimal Quantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    public IReadOnlyCollection<FormulaRequirement> Requirements => _requirements.AsReadOnly();

    private FormulaHeader() { }

    public static FormulaHeader Create(int itemId, int version, decimal quantity, int? createdByUserId)
    {
        if (itemId <= 0) throw new ArgumentOutOfRangeException(nameof(itemId));
        if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        var formula = new FormulaHeader
        {
            ItemId = itemId,
            Version = version,
            Quantity = quantity,
            IsActive = true,
            CreatedByUserId = createdByUserId
        };
        formula.RaiseDomainEvent(new FormulaChanged(FormulaChangeTypes.Created, DateTime.UtcNow));
        return formula;
    }

    public void Update(decimal quantity, int modifiedByUserId)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        Quantity = quantity;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new FormulaChanged(FormulaChangeTypes.Updated, DateTime.UtcNow));
    }

    public void AddRequirement(int rawMaterialId, decimal quantity)
    {
        if (rawMaterialId <= 0) throw new ArgumentOutOfRangeException(nameof(rawMaterialId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        if (_requirements.Any(r => r.RawMaterialId == rawMaterialId))
            throw new InvalidOperationException($"A requirement for item {rawMaterialId} already exists.");

        _requirements.Add(FormulaRequirement.Create(Id, rawMaterialId, quantity));
        RaiseDomainEvent(new FormulaChanged(FormulaChangeTypes.Updated, DateTime.UtcNow));
    }

    public void RemoveRequirement(int rawMaterialId)
    {
        var existing = _requirements.FirstOrDefault(r => r.RawMaterialId == rawMaterialId);
        if (existing is null) return;

        _requirements.Remove(existing);
        RaiseDomainEvent(new FormulaChanged(FormulaChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;

        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new FormulaChanged(
            active ? FormulaChangeTypes.Activated : FormulaChangeTypes.Deactivated, DateTime.UtcNow));
    }
}