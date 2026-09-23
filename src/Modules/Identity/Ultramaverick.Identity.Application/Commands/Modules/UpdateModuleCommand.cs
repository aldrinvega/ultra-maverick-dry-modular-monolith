using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Modules
{
    public sealed record UpdateModuleCommand(int ModuleId, int MainMenuId, string Name, string? SubMenuName)
        : IRequest<Result>;
}
