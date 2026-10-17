using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class ItemCategory : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private ItemCategory() { }

    public static ItemCategory Create(string name, int? createdByUserId)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = new ItemCategory { Name = name, IsActive = true, CreatedByUserId = createdByUserId };
        entity.RaiseDomainEvent(new ItemCategoryChanged(ItemCategoryChangeTypes.Created, DateTime.UtcNow));
        return entity;
    }

    public void Rename(string name, int modifiedByUserId)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new ItemCategoryChanged(ItemCategoryChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;

        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new ItemCategoryChanged(
            active ? ItemCategoryChangeTypes.Activated : ItemCategoryChangeTypes.Deactivated, DateTime.UtcNow));
    }
}