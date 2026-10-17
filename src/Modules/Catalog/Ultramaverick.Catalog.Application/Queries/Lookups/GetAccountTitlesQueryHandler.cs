using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Lookups;

public sealed class GetAccountTitlesQueryHandler : IRequestHandler<GetAccountTitlesQuery, PagedResult<LookupDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetAccountTitlesQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<LookupDto>> Handle(GetAccountTitlesQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var rows = await _uow.AccountTitles.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
        var total = await _uow.AccountTitles.CountAsync(request.Search, request.IsActive, ct);

        var items = rows
            .Select(x => new LookupDto(x.Id, x.Code, x.Name, x.IsActive))
            .ToList();

        return new PagedResult<LookupDto>(items, page, pageSize, total);
    }
}