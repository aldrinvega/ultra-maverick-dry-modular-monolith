using Ultramaverick.Quality.Application.Queries.Checklists;

namespace Ultramaverick.Quality.Application.Abstractions;

public interface IChecklistQuestionReader
{
    Task<IReadOnlyList<ChecklistQuestionDto>> GetByTypeAsync(int checklistTypeId, CancellationToken ct);
}
