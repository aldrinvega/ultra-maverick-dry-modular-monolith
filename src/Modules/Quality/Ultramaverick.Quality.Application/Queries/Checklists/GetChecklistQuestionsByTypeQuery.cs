using MediatR;
using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Application.Models;

namespace Ultramaverick.Quality.Application.Queries.Checklists;

public sealed record ChecklistQuestionDto(
    int Id,
    int ChecklistTypeId,
    string Question,
    int AnswerType,
    int SortOrder,
    bool IsActive);

public sealed record GetChecklistQuestionsByTypeQuery(int ChecklistTypeId)
    : IRequest<Result<IReadOnlyList<ChecklistQuestionDto>>>;

public sealed class GetChecklistQuestionsByTypeQueryHandler
    : IRequestHandler<GetChecklistQuestionsByTypeQuery, Result<IReadOnlyList<ChecklistQuestionDto>>>
{
    private readonly IChecklistQuestionReader _reader;

    public GetChecklistQuestionsByTypeQueryHandler(IChecklistQuestionReader reader) => _reader = reader;

    public async Task<Result<IReadOnlyList<ChecklistQuestionDto>>> Handle(
        GetChecklistQuestionsByTypeQuery request, CancellationToken ct)
    {
        if (request.ChecklistTypeId <= 0)
            return Result<IReadOnlyList<ChecklistQuestionDto>>.Failure(
                "Checklist type id must be greater than zero.");

        var questions = await _reader.GetByTypeAsync(request.ChecklistTypeId, ct);
        return Result<IReadOnlyList<ChecklistQuestionDto>>.Success(questions);
    }
}
