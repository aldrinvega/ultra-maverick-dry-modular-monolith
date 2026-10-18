using Ultramaverick.SharedKernel;

namespace Ultramaverick.Orders.Domain.Entities;

/// <summary>Map-only, immutable record of a completed move. No RowVersion by design.</summary>
public sealed class TransactedMoveOrder : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int MoveOrderId { get; private set; }
    public int? OrderNo { get; private set; }
    public int? PreparedByUserId { get; private set; }
    public DateTime? PreparedAtUtc { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public bool IsTransact { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private TransactedMoveOrder() { }
}
