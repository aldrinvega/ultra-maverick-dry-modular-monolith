using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

public sealed class ChecklistOpenFieldAnswer : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ChecklistId { get; private set; }
    public int QuestionId { get; private set; }
    public string Remarks { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private ChecklistOpenFieldAnswer() { }

    internal static ChecklistOpenFieldAnswer Create(int checklistId, int questionId, string remarks)
        => new() { ChecklistId = checklistId, QuestionId = questionId, Remarks = remarks, IsActive = true };
}
