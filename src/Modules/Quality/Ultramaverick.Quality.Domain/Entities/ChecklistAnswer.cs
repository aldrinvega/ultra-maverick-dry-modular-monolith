using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

public sealed class ChecklistAnswer : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ChecklistId { get; private set; }
    public int QuestionId { get; private set; }
    public bool Status { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private ChecklistAnswer() { }

    internal static ChecklistAnswer Create(int checklistId, int questionId, bool status)
        => new() { ChecklistId = checklistId, QuestionId = questionId, Status = status, IsActive = true };
}
