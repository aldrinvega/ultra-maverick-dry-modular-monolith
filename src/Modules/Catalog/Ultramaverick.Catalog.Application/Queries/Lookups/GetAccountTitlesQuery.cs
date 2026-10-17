using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Lookups;

public sealed record GetAccountTitlesQuery(int Page, int PageSize, string? Search, bool? IsActive)
    : IRequest<PagedResult<LookupDto>>;