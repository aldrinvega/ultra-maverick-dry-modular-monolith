using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Items;

public sealed record GetItemsQuery(int Page, int PageSize, string? Search, bool? IsActive)
    : IRequest<PagedResult<ItemDto>>;