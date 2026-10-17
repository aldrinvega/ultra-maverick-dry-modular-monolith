using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class Location : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string? Code { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private Location() { }

    public static Location Create(string name, int? createdByUserId, string? code = null)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = new Location
        {
            Name = name,
            Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim(),
            IsActive = true,
            CreatedByUserId = createdByUserId
        };
        entity.RaiseDomainEvent(new LocationChanged(LocationChangeTypes.Created, DateTime.UtcNow));
        return entity;
    }

    public void Rename(string name, string? code, int modifiedByUserId)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new LocationChanged(LocationChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;

        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new LocationChanged(
            active ? LocationChangeTypes.Activated : LocationChangeTypes.Deactivated, DateTime.UtcNow));
    }
}