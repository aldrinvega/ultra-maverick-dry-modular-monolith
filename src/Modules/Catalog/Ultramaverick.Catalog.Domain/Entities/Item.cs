using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class Item : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string ItemCode { get; private set; } = null!;
    public string ItemDescription { get; private set; } = null!;
    public int ItemCategoryId { get; private set; }
    public int UomId { get; private set; }
    public decimal? BufferLevel { get; private set; }
    public bool IsExpirable { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private Item() { }

    public static Item Create(
        string itemCode, string itemDescription, int itemCategoryId, int uomId,
        decimal? bufferLevel, bool isExpirable, int? createdByUserId)
    {
        itemCode = (itemCode ?? string.Empty).Trim();
        itemDescription = (itemDescription ?? string.Empty).Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(itemCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(itemDescription);
        if (itemCategoryId <= 0) throw new ArgumentOutOfRangeException(nameof(itemCategoryId));
        if (uomId <= 0) throw new ArgumentOutOfRangeException(nameof(uomId));

        var item = new Item
        {
            ItemCode = itemCode,
            ItemDescription = itemDescription,
            ItemCategoryId = itemCategoryId,
            UomId = uomId,
            BufferLevel = bufferLevel,
            IsExpirable = isExpirable,
            IsActive = true,
            CreatedByUserId = createdByUserId
        };

        item.RaiseDomainEvent(new ItemChanged(ItemChangeTypes.Created, DateTime.UtcNow));
        return item;
    }

    public void Update(
        string itemDescription, int itemCategoryId, int uomId,
        decimal? bufferLevel, bool isExpirable, int modifiedByUserId)
    {
        itemDescription = (itemDescription ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(itemDescription);
        if (itemCategoryId <= 0) throw new ArgumentOutOfRangeException(nameof(itemCategoryId));
        if (uomId <= 0) throw new ArgumentOutOfRangeException(nameof(uomId));

        ItemDescription = itemDescription;
        ItemCategoryId = itemCategoryId;
        UomId = uomId;
        BufferLevel = bufferLevel;
        IsExpirable = isExpirable;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;

        RaiseDomainEvent(new ItemChanged(ItemChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;

        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new ItemChanged(
            active ? ItemChangeTypes.Activated : ItemChangeTypes.Deactivated, DateTime.UtcNow));
    }
}