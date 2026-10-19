using Microsoft.EntityFrameworkCore;
using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Application.Queries.Checklists;

namespace Ultramaverick.Quality.Persistence.Repositories;

public sealed class ChecklistQuestionReader : IChecklistQuestionReader
{
    private readonly QualityDbContext _context;

    public ChecklistQuestionReader(QualityDbContext context) => _context = context;

    public async Task<IReadOnlyList<ChecklistQuestionDto>> GetByTypeAsync(
        int checklistTypeId, CancellationToken ct)
        => await _context.ChecklistQuestions
            .AsNoTracking()
            .Where(x => x.ChecklistTypeId == checklistTypeId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new ChecklistQuestionDto(
                x.Id, x.ChecklistTypeId, x.Question, x.AnswerType, x.SortOrder, x.IsActive))
            .ToListAsync(ct);
}
