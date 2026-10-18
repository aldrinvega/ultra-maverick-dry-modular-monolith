using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

/// <summary>Answer kind for a checklist question (§7.1: 0=YesNo, 1=OpenField, 2=Dimensions).</summary>
public enum ChecklistAnswerType
{
    YesNo = 0,
    OpenField = 1,
    Dimensions = 2
}

/// <summary>
/// A question belonging to a checklist template (§7.1). Derived from the §7.1 table note
/// "ChecklistTypeId (FK), question, AnswerType (0=YesNo,1=OpenField,2=Dimensions)";
/// SortOrder and IsActive drive ordered, soft-deletable question lists.
/// </summary>
public sealed class ChecklistQuestion : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int ChecklistTypeId { get; private set; }
    public string Question { get; private set; } = null!;
    public int AnswerType { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private ChecklistQuestion() { }

    public static ChecklistQuestion Create(
        int checklistTypeId, string question, ChecklistAnswerType answerType, int sortOrder)
    {
        if (checklistTypeId <= 0) throw new ArgumentOutOfRangeException(nameof(checklistTypeId));
        question = (question ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        return new ChecklistQuestion
        {
            ChecklistTypeId = checklistTypeId, Question = question,
            AnswerType = (int)answerType, SortOrder = sortOrder, IsActive = true
        };
    }
}
