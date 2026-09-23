using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.MainMenus
{
    public sealed record GetMainMenuByIdQuery(int MenuId) : IRequest<Result<MainMenuDto>>;
}
