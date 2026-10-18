using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

/// <summary>
/// The review/verify decision and disposition for a filled checklist (§7.1). Derived from
/// the §7.1 note "the review/verify decision and disposition": the disposition plus the
/// reviewer and the time the decision was logged.
/// </summary>
public sealed class ChecklistReviewVerificationLog : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ChecklistId { get; private set; }
    public string Disposition { get; private set; } = null!;
    public string? Remarks { get; private set; }
    public int? ReviewedByUserId { get; private set; }
    public DateTime ReviewedAtUtc { get; private set; }

    private ChecklistReviewVerificationLog() { }

    public static ChecklistReviewVerificationLog Create(
        int checklistId, string disposition, string? remarks, int? reviewedByUserId)
    {
        if (checklistId <= 0) throw new ArgumentOutOfRangeException(nameof(checklistId));
        disposition = (disposition ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(disposition);

        return new ChecklistReviewVerificationLog
        {
            ChecklistId = checklistId, Disposition = disposition, Remarks = remarks,
            ReviewedByUserId = reviewedByUserId, ReviewedAtUtc = DateTime.UtcNow
        };
    }
}
