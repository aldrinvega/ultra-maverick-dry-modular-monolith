using Ultramaverick.SharedKernel;

namespace Ultramaverick.Orders.Domain.Entities;

/// <summary>Map-only placeholder for a stock move instruction. No behaviour this wave.</summary>
public sealed class MoveOrder : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int? OrderId { get; private set; }
    public int? OrderLineId { get; private set; }
    public int? OrderNo { get; private set; }
    public int? FarmId { get; private set; }                // scalar → Catalog (no FK)
    public int ItemId { get; private set; }                 // scalar → Catalog (no FK)
    public int UomId { get; private set; }                  // scalar → Catalog (no FK)
    public decimal QuantityOrdered { get; private set; }
    public DateTime? OrderDate { get; private set; }
    public DateTime? DateNeeded { get; private set; }
    public int? WarehouseId { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public string Status { get; private set; } = "Pending";
    public bool IsApproved { get; private set; }
    public int? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }
    public bool IsPrepared { get; private set; }
    public int? PreparedByUserId { get; private set; }
    public DateTime? PreparedAtUtc { get; private set; }
    public bool IsCancelled { get; private set; }
    public int? CancelledByUserId { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public bool IsRejected { get; private set; }
    public int? RejectedByUserId { get; private set; }
    public DateTime? RejectedAtUtc { get; private set; }
    public string? PlateNumber { get; private set; }
    public string? DeliveryStatus { get; private set; }
    public int? AccountTitleId { get; private set; }        // scalar → Catalog (no FK)
    public int? LocationId { get; private set; }            // scalar → Catalog (no FK)
    public int? DepartmentId { get; private set; }          // scalar → Identity (no FK)
    public string? CompanyCode { get; private set; }
    public string? CompanyName { get; private set; }
    public int? AdvancesToEmployeeId { get; private set; }
    public int? CheckedByUserId { get; private set; }
    public string? Remarks { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private MoveOrder() { }
}
