using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.MainMenus
{
    public sealed record UpdateMainMenuCommand(int MenuId, string Name, string Path, int SortOrder)
        : IRequest<Result>;
}
