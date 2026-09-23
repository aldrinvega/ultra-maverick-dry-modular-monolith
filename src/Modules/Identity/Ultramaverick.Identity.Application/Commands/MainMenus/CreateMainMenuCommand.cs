using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.MainMenus
{
    public sealed record CreateMainMenuCommand(string Name, string Path, int SortOrder) : IRequest<Result<int>>;
}
