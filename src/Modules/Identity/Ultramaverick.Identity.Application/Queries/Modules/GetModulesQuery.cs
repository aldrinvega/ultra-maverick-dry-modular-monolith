using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Modules
{
    public sealed record GetModulesQuery(int Page, int PageSize, string? Search, bool? IsActive)
        : IRequest<PagedResult<ModuleDto>>;
}
