using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Items;

public sealed class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, Result<ItemDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetItemByIdQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<Result<ItemDto>> Handle(GetItemByIdQuery request, CancellationToken ct)
    {
        var item = await _uow.Items.GetByIdAsync(request.ItemId, ct);
        if (item is null) return Result<ItemDto>.Failure("Item not found.");

        var category = await _uow.ItemCategories.GetByIdAsync(item.ItemCategoryId, ct);
        var uom = await _uow.Uoms.GetByIdAsync(item.UomId, ct);

        var dto = new ItemDto(
            item.Id,
            item.ItemCode,
            item.ItemDescription,
            item.ItemCategoryId,
            category?.Name ?? string.Empty,
            item.UomId,
            uom?.Description ?? string.Empty,
            item.BufferLevel,
            item.IsExpirable,
            item.IsActive);

        return Result<ItemDto>.Success(dto);
    }
}