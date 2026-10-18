using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

public sealed class ChecklistProductDimension : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ChecklistId { get; private set; }
    public int QuestionId { get; private set; }
    public decimal? StandardValue { get; private set; }
    public decimal? ActualValue { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private ChecklistProductDimension() { }

    internal static ChecklistProductDimension Create(int checklistId, int questionId, decimal? standard, decimal? actual)
        => new()
        {
            ChecklistId = checklistId, QuestionId = questionId,
            StandardValue = standard, ActualValue = actual, IsActive = true
        };
}
