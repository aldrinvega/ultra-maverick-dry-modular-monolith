using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Reasons;

public sealed class GetReasonsQueryHandler : IRequestHandler<GetReasonsQuery, PagedResult<ReasonDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetReasonsQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<ReasonDto>> Handle(GetReasonsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var rows = await _uow.Reasons.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
        var total = await _uow.Reasons.CountAsync(request.Search, request.IsActive, ct);

        var items = rows.Select(r => new ReasonDto(r.Id, r.Name, r.MenuId, r.IsActive)).ToList();

        return new PagedResult<ReasonDto>(items, page, pageSize, total);
    }
}