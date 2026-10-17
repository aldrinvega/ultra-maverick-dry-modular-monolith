using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class Disposition : Entity
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

    private Disposition() { }

    public static Disposition Create(string name, int? createdByUserId, string? code = null)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = new Disposition
        {
            Name = name,
            Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim(),
            IsActive = true,
            CreatedByUserId = createdByUserId
        };
        entity.RaiseDomainEvent(new DispositionChanged(DispositionChangeTypes.Created, DateTime.UtcNow));
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
        RaiseDomainEvent(new DispositionChanged(DispositionChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;

        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new DispositionChanged(
            active ? DispositionChangeTypes.Activated : DispositionChangeTypes.Deactivated, DateTime.UtcNow));
    }
}