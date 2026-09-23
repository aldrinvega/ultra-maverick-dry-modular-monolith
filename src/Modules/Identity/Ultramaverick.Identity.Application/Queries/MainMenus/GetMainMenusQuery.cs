using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.MainMenus
{
    public sealed record GetMainMenusQuery(int Page, int PageSize, string? Search, bool? IsActive)
        : IRequest<PagedResult<MainMenuDto>>;
}
