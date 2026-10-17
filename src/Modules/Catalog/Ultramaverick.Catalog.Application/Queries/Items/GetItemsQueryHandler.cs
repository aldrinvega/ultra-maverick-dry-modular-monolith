using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Queries.Items;

public sealed class GetItemsQueryHandler : IRequestHandler<GetItemsQuery, PagedResult<ItemDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetItemsQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<ItemDto>> Handle(GetItemsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var items = await _uow.Items.ListAsync(request.Search, request.IsActive, skip, pageSize, ct);
        var total = await _uow.Items.CountAsync(request.Search, request.IsActive, ct);

        var items2 = await MapAsync(items, ct);

        return new PagedResult<ItemDto>(items2, page, pageSize, total);
    }

    private async Task<IReadOnlyList<ItemDto>> MapAsync(IReadOnlyList<Item> items, CancellationToken ct)
    {
        var categoryNames = new Dictionary<int, string>();
        foreach (var id in items.Select(i => i.ItemCategoryId).Distinct())
        {
            var category = await _uow.ItemCategories.GetByIdAsync(id, ct);
            if (category is not null) categoryNames[id] = category.Name;
        }

        var uomDescriptions = new Dictionary<int, string>();
        foreach (var id in items.Select(i => i.UomId).Distinct())
        {
            var uom = await _uow.Uoms.GetByIdAsync(id, ct);
            if (uom is not null) uomDescriptions[id] = uom.Description;
        }

        return items
            .Select(i => new ItemDto(
                i.Id,
                i.ItemCode,
                i.ItemDescription,
                i.ItemCategoryId,
                categoryNames.TryGetValue(i.ItemCategoryId, out var cn) ? cn : string.Empty,
                i.UomId,
                uomDescriptions.TryGetValue(i.UomId, out var ud) ? ud : string.Empty,
                i.BufferLevel,
                i.IsExpirable,
                i.IsActive))
            .ToList();
    }
}