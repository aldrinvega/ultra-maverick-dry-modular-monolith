using Ultramaverick.SharedKernel;

namespace Ultramaverick.Warehouse.Domain.Entities;

/// <summary>Map-only placeholder for a manual stock issue. No behaviour this wave (G41).</summary>
public sealed class MiscellaneousIssue : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int WarehouseId { get; private set; }
    public string? RefNo { get; private set; }
    public DateTime IssuedAtUtc { get; private set; } = DateTime.UtcNow;
    public string? Remarks { get; private set; }
    public int? IssuedByUserId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<MiscellaneousIssueLine> Lines { get; private set; } = new List<MiscellaneousIssueLine>();

    private MiscellaneousIssue() { }
}
