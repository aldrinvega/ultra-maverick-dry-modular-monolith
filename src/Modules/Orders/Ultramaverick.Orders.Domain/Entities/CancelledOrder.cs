using Ultramaverick.SharedKernel;

namespace Ultramaverick.Orders.Domain.Entities;

/// <summary>Map-only record of a cancelled order. Not written by any use case this wave.</summary>
public sealed class CancelledOrder : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int OrderId { get; private set; }
    public int? CustomerId { get; private set; }            // scalar → Catalog (no FK)
    public int? ReasonId { get; private set; }              // scalar → Catalog (no FK)
    public int? CancelledByUserId { get; private set; }
    public DateTime CancelledAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private CancelledOrder() { }
}
