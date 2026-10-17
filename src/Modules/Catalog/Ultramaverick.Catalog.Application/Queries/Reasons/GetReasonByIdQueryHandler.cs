using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Reasons;

public sealed class GetReasonByIdQueryHandler : IRequestHandler<GetReasonByIdQuery, Result<ReasonDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetReasonByIdQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<Result<ReasonDto>> Handle(GetReasonByIdQuery request, CancellationToken ct)
    {
        var reason = await _uow.Reasons.GetByIdAsync(request.ReasonId, ct);
        if (reason is null) return Result<ReasonDto>.Failure("Reason not found.");

        return Result<ReasonDto>.Success(new ReasonDto(reason.Id, reason.Name, reason.MenuId, reason.IsActive));
    }
}