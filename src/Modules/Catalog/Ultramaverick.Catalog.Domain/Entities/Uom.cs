using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class Uom : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private Uom() { }

    public static Uom Create(string code, string description, int? createdByUserId)
    {
        code = (code ?? string.Empty).Trim();
        description = (description ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var uom = new Uom
        {
            Code = code,
            Description = description,
            IsActive = true,
            CreatedByUserId = createdByUserId
        };
        uom.RaiseDomainEvent(new UomChanged(UomChangeTypes.Created, DateTime.UtcNow));
        return uom;
    }

    public void Update(string code, string description, int modifiedByUserId)
    {
        code = (code ?? string.Empty).Trim();
        description = (description ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Code = code;
        Description = description;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new UomChanged(UomChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;

        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new UomChanged(
            active ? UomChangeTypes.Activated : UomChangeTypes.Deactivated, DateTime.UtcNow));
    }
}