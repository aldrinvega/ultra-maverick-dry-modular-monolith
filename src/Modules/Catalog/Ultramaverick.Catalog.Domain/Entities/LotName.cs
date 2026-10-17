using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class LotName : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string Code { get; private set; } = null!;
    public int LotCategoryId { get; private set; }
    public string? SectionName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private LotName() { }

    public static LotName Create(string code, int lotCategoryId, string? sectionName, int? createdByUserId)
    {
        code = (code ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (lotCategoryId <= 0) throw new ArgumentOutOfRangeException(nameof(lotCategoryId));

        var lotName = new LotName
        {
            Code = code,
            LotCategoryId = lotCategoryId,
            SectionName = string.IsNullOrWhiteSpace(sectionName) ? null : sectionName.Trim(),
            IsActive = true,
            CreatedByUserId = createdByUserId
        };
        lotName.RaiseDomainEvent(new LotNameChanged(LotNameChangeTypes.Created, DateTime.UtcNow));
        return lotName;
    }

    public void Update(string code, int lotCategoryId, string? sectionName, int modifiedByUserId)
    {
        code = (code ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (lotCategoryId <= 0) throw new ArgumentOutOfRangeException(nameof(lotCategoryId));

        Code = code;
        LotCategoryId = lotCategoryId;
        SectionName = string.IsNullOrWhiteSpace(sectionName) ? null : sectionName.Trim();
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new LotNameChanged(LotNameChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;

        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new LotNameChanged(
            active ? LotNameChangeTypes.Activated : LotNameChangeTypes.Deactivated, DateTime.UtcNow));
    }
}