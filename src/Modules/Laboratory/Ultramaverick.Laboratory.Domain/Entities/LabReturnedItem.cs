using Ultramaverick.SharedKernel;

namespace Ultramaverick.Laboratory.Domain.Entities;

public sealed class LabReturnedItem : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int LabReceiveRequestId { get; private set; }
    public decimal? Quantity { get; private set; }
    public int? ReasonId { get; private set; }
    public string? Remarks { get; private set; }
    public int? ReturnedByUserId { get; private set; }
    public DateTime? ReturnedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private LabReturnedItem() { }

    internal static LabReturnedItem Create(
        int labReceiveRequestId, decimal? quantity, int? reasonId, string? remarks,
        int? returnedByUserId, DateTime? returnedAtUtc)
        => new()
        {
            LabReceiveRequestId = labReceiveRequestId,
            Quantity = quantity,
            ReasonId = reasonId,
            Remarks = remarks,
            ReturnedByUserId = returnedByUserId,
            ReturnedAtUtc = returnedAtUtc,
            IsActive = true
        };
}
