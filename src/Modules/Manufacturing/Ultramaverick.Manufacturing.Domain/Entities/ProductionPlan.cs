using Ultramaverick.SharedKernel;

namespace Ultramaverick.Manufacturing.Domain.Entities;

/// <summary>
/// A production plan owned by Manufacturing. Map-only this wave: it exists so transformation
/// requests can reference a real plan row, but no command writes it yet.
/// </summary>
public sealed class ProductionPlan : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ItemId { get; private set; }  // scalar → Catalog (no FK)
    public int UomId { get; private set; }   // scalar → Catalog (no FK)
    public string? ProdPlanNo { get; private set; }
    public int? Version { get; private set; }
    public string? Batch { get; private set; }
    public decimal Quantity { get; private set; }
    public string Status { get; private set; } = "Pending";
    public bool IsApproved { get; private set; }
    public int? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }
    public bool IsPrepared { get; private set; }
    public int? RejectedByUserId { get; private set; }
    public DateTime? RejectedAtUtc { get; private set; }
    public string? RejectRemarks { get; private set; }
    public string? CancelRemarks { get; private set; }
    public bool IsMixed { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private ProductionPlan() { }
}
