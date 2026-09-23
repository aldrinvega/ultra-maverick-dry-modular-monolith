using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Roles
{
    public sealed record GetRolesQuery(int Page, int PageSize, string? Search, bool? IsActive)
        : IRequest<PagedResult<RoleDto>>;
}
