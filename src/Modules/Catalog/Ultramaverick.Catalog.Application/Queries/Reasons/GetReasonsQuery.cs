using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Reasons;

public sealed record GetReasonsQuery(int Page, int PageSize, string? Search, bool? IsActive)
    : IRequest<PagedResult<ReasonDto>>;