using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Uoms;

public sealed record GetUomsQuery(int Page, int PageSize, string? Search, bool? IsActive)
    : IRequest<PagedResult<UomDto>>;