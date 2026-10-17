using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class Reason : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string Name { get; private set; } = null!;
    public int? MenuId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private Reason() { }

    public static Reason Create(string name, int? menuId, int? createdByUserId)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var reason = new Reason
        {
            Name = name,
            MenuId = menuId,
            IsActive = true,
            CreatedByUserId = createdByUserId
        };
        reason.RaiseDomainEvent(new ReasonChanged(ReasonChangeTypes.Created, DateTime.UtcNow));
        return reason;
    }

    public void Update(string name, int? menuId, int modifiedByUserId)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        MenuId = menuId;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new ReasonChanged(ReasonChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;

        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new ReasonChanged(
            active ? ReasonChangeTypes.Activated : ReasonChangeTypes.Deactivated, DateTime.UtcNow));
    }
}