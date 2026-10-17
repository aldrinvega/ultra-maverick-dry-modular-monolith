using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.LotNames;

public sealed class GetLotNamesQueryHandler : IRequestHandler<GetLotNamesQuery, PagedResult<LotNameDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetLotNamesQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<LotNameDto>> Handle(GetLotNamesQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var rows = await _uow.LotNames.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
        var total = await _uow.LotNames.CountAsync(request.Search, request.IsActive, ct);

        var categoryNames = new Dictionary<int, string>();
        foreach (var id in rows.Select(l => l.LotCategoryId).Distinct())
        {
            var category = await _uow.LotCategories.GetByIdAsync(id, ct);
            if (category is not null) categoryNames[id] = category.Name;
        }

        var items = rows
            .Select(l => new LotNameDto(
                l.Id, l.Code, l.LotCategoryId,
                categoryNames.TryGetValue(l.LotCategoryId, out var cn) ? cn : string.Empty,
                l.SectionName, l.IsActive))
            .ToList();

        return new PagedResult<LotNameDto>(items, page, pageSize, total);
    }
}