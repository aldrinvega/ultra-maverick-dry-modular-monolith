using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.LotNames;

public sealed record GetLotNamesQuery(int Page, int PageSize, string? Search, bool? IsActive)
    : IRequest<PagedResult<LotNameDto>>;