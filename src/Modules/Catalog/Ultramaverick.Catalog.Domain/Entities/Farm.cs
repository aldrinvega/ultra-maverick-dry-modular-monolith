using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class Farm : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string FarmCode { get; private set; } = null!;
    public string FarmName { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private Farm() { }

    public static Farm Create(string code, string name, int? createdByUserId)
    {
        code = (code ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var farm = new Farm
        {
            FarmCode = code,
            FarmName = name,
            IsActive = true,
            CreatedByUserId = createdByUserId
        };
        farm.RaiseDomainEvent(new FarmChanged(FarmChangeTypes.Created, DateTime.UtcNow));
        return farm;
    }

    public void Update(string code, string name, int modifiedByUserId)
    {
        code = (code ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        FarmCode = code;
        FarmName = name;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new FarmChanged(FarmChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;

        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new FarmChanged(
            active ? FarmChangeTypes.Activated : FarmChangeTypes.Deactivated, DateTime.UtcNow));
    }
}