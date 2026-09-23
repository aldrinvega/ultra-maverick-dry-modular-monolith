using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.MainMenus
{
    public sealed record SetMainMenuActiveCommand(int MenuId, bool IsActive) : IRequest<Result>;
}
