using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Uoms;

public sealed class GetUomsQueryHandler : IRequestHandler<GetUomsQuery, PagedResult<UomDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetUomsQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<UomDto>> Handle(GetUomsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var rows = await _uow.Uoms.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
        var total = await _uow.Uoms.CountAsync(request.Search, request.IsActive, ct);

        var items = rows.Select(u => new UomDto(u.Id, u.Code, u.Description, u.IsActive)).ToList();

        return new PagedResult<UomDto>(items, page, pageSize, total);
    }
}