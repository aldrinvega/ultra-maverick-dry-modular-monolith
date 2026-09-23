using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Users
{
    public sealed record GetUsersQuery(int Page, int PageSize, string? Search, bool? IsActive)
        : IRequest<PagedResult<UserDto>>;
}
